using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Net;
using System.Net.Mail;
using TeknikServis_CRM.Models;

using Microsoft.EntityFrameworkCore;

namespace TeknikServis_CRM.Controllers
{
    public class AccountController : Controller
    {
        private readonly CrmDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(CrmDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Index() => View();

        // --- 1. OTURUM BİLGİSİ GÜNCELLEME (HIZLANDIRILDI) ---
        private async Task GirisBilgilendirme(int kullaniciId)
        {
            // Cihaz tespiti
            string userAgent = Request.Headers["User-Agent"].ToString();
            string cihazTipi = userAgent.Contains("Android") ? "Android" :
                               userAgent.Contains("iPhone") ? "iPhone" :
                               userAgent.Contains("Macintosh") ? "MacBook" : "Windows PC";

            // IP Tespiti
            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            // Performans için dış API (ip-api) kaldırıldı, sadece IP ve Cihaz bilgisi tutuluyor
            string fullIpInfo = $"{ip} | {cihazTipi}";

            var mevcutOturum = await _context.KullaniciOturumlaris.FirstOrDefaultAsync(x => x.KullaniciId == kullaniciId);

            if (mevcutOturum != null)
            {
                mevcutOturum.IpAdresi = fullIpInfo;
                mevcutOturum.GirisTarihi = DateTime.Now;
                mevcutOturum.Token = Guid.NewGuid().ToString();
                mevcutOturum.AktifMi = true;
                _context.KullaniciOturumlaris.Update(mevcutOturum);
            }
            else
            {
                _context.KullaniciOturumlaris.Add(new KullaniciOturumlari
                {
                    KullaniciId = kullaniciId,
                    GirisTarihi = DateTime.Now,
                    AktifMi = true,
                    Token = Guid.NewGuid().ToString(),
                    IpAdresi = fullIpInfo
                });
            }
            // Not: SaveChanges Login metodunda toplu yapılacak, burada await çağırmıyoruz.
        }

        // --- 2. GİRİŞ YAPMA ---
        [HttpPost]
        public async Task<IActionResult> Login(string kullaniciAdi, string sifre, bool beniHatirla)
        {
            string hash = HomeController.SifreleSHA256(sifre);

            var user = await _context.Kullanicilars
                .FirstOrDefaultAsync(u => u.KullaniciAdi == kullaniciAdi && u.SifreHash == hash && !u.SilindiMi);

            if (user != null)
            {
                var roller = await _context.KullaniciRolleris
                    .Where(kr => kr.KullaniciId == user.Id)
                    .Join(_context.Rollers, kr => kr.RolId, r => r.Id, (kr, r) => r.RolAdi)
                    .ToListAsync();

                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, user.KullaniciAdi),
                    new Claim("FullName", user.Ad + " " + user.Soyad),
                    new Claim("UserId", user.Id.ToString())
                };

                foreach (var rol in roller)
                {
                    claims.Add(new Claim(ClaimTypes.Role, rol));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProps = new AuthenticationProperties { IsPersistent = beniHatirla };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProps);

                // Bilgilendirme kaydını hazırla
                await GirisBilgilendirme(user.Id);

                // Son giriş tarihini güncelle
                user.SonGirisTarihi = DateTime.Now;
                _context.Kullanicilars.Update(user);

                // Tüm veritabanı değişikliklerini tek seferde kaydet (Performans için kritik)
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("FullName", user.Ad + " " + user.Soyad);

                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }
            return Json(new { success = false, message = "Kullanıcı adı veya şifre hatalı!" });
        }

        // --- 3. ÇIKIŞ YAPMA ---
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Account");
        }

        // --- 4. KAYIT OLMA ---
        [HttpPost]
        public async Task<IActionResult> Register(Kullanicilar model, string SifreInput)
        {
            if (await _context.Kullanicilars.AnyAsync(u => u.KullaniciAdi == model.KullaniciAdi || u.Eposta == model.Eposta))
                return Json(new { success = false, message = "Bu bilgiler zaten kullanımda!" });

            model.SifreHash = HomeController.SifreleSHA256(SifreInput);
            model.Durum = "Aktif";
            model.OlusturmaTarihi = DateTime.Now;
            model.SilindiMi = false;
            model.IsOnayli = true;

            _context.Kullanicilars.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Kayıt başarılı!" });
        }

        // --- 5. ŞİFREMİ UNUTTUM ---
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Kullanicilars.FirstOrDefaultAsync(u => u.Eposta == email && !u.SilindiMi);
            if (user == null) return Json(new { success = false, message = "E-posta bulunamadı!" });

            string code = new Random().Next(100000, 999999).ToString();
            TempData["ResetCode"] = code;
            TempData["ResetEmail"] = email;

            bool mailSent = await MailGonderKodAsync(email, code, "Güvenlik Onay Kodu");
            return mailSent ? Json(new { success = true, email = email }) : Json(new { success = false, message = "Mail hatası!" });
        }

        // --- 6. ŞİFRE SIFIRLAMA ---
        [HttpPost]
        public async Task<IActionResult> ResetPassword(string inputCode, string newPassword)
        {
            string sessionCode = TempData["ResetCode"]?.ToString();
            string email = TempData["ResetEmail"]?.ToString();

            if (inputCode == sessionCode && !string.IsNullOrEmpty(email))
            {
                var user = await _context.Kullanicilars.FirstOrDefaultAsync(u => u.Eposta == email);
                if (user != null)
                {
                    user.SifreHash = HomeController.SifreleSHA256(newPassword);
                    _context.Kullanicilars.Update(user);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Şifreniz güncellendi!" });
                }
            }
            return Json(new { success = false, message = "Kod geçersiz!" });
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        // --- 7. MAİL GÖNDERME (TAM ASENKRON) ---
        private async Task<bool> MailGonderKodAsync(string aliciEmail, string kod, string konu)
        {
            try
            {
                var senderEmail = "beyzakblt@gmail.com";
                var appPassword = "eqvmlqjbubnmhdok";
                string htmlBody = $"<div style='padding:20px; border:1px solid #eee;'><h2>Kodunuz: {kod}</h2></div>";

                using var smtp = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(senderEmail, appPassword),
                    EnableSsl = true
                };

                var mail = new MailMessage(senderEmail, aliciEmail, konu, htmlBody) { IsBodyHtml = true };
                await smtp.SendMailAsync(mail); // Await kullanımı sayesinde UI donmaz
                return true;
            }
            catch { return false; }
        }

        // --- 8. İLK ADMİN OLUŞTURMA ---
        [HttpGet]
        public async Task<IActionResult> IlkKodum()
        {
            if (!await _context.Kullanicilars.AnyAsync(u => u.KullaniciAdi == "admin"))
            {
                var ilkUser = new Kullanicilar
                {
                    Ad = "Beyza",
                    Soyad = "Akbulut",
                    KullaniciAdi = "admin",
                    Eposta = "beyzakblt@gmail.com",
                    SifreHash = HomeController.SifreleSHA256("123456"),
                    Durum = "Aktif",
                    OlusturmaTarihi = DateTime.Now,
                    SilindiMi = false,
                    IsOnayli = true
                };
                _context.Kullanicilars.Add(ilkUser);
                await _context.SaveChangesAsync();
                return Content("Admin OK (123456).");
            }
            return Content("Admin zaten var.");
        }
    }
}