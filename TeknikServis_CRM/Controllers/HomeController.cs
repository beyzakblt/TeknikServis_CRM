using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TeknikServis_CRM.Models;
using TeknikServis_CRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace TeknikServis_CRM.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CrmDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(ILogger<HomeController> logger, CrmDbContext context, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // --- 0. MERKEZİ ŞİFRELEME METODU (DİĞER CONTROLLERLAR BURADAN ÇAĞIRACAK) ---
        public static string SifreleSHA256(string hamSifre)
        {
            if (string.IsNullOrEmpty(hamSifre)) return "";
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hamSifre));
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }

        // --- 1. DASHBOARD ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var fullName = HttpContext.Session.GetString("FullName");
            ViewBag.UserFullName = !string.IsNullOrEmpty(fullName) ? fullName : (User.Identity?.Name ?? "Kullanıcı");

            string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            if (clientIp == "::1" || clientIp == "127.0.0.1") clientIp = "176.234.224.120";

            try
            {
                var client = _httpClientFactory.CreateClient();
                var location = await client.GetFromJsonAsync<IpLocationResult>($"http://ip-api.com/json/{clientIp}?fields=status,city,regionName");

                if (location != null && location.status == "success")
                {
                    ViewBag.City = location.city;
                    ViewBag.District = location.regionName;
                }
            }
            catch
            {
                ViewBag.City = "Bilinmiyor";
                ViewBag.District = "Servis Hatası";
            }

            ViewBag.TotalCihaz = await _context.Cihazlars.CountAsync();
            ViewBag.TotalUsers = await _context.Kullanicilars.CountAsync(x => !x.SilindiMi);

            return View();
        }

        // --- 2. KULLANICILARI LİSTELE (MODAL İÇİN ROLLER DAHİL) ---
        [HttpGet]
        public async Task<IActionResult> Kullanicilar()
        {
            // Veritabanındaki aktif rolleri ViewBag'e atıyoruz (Dinamik Liste)
            ViewBag.Roller = await _context.Rollers.Where(r => r.Durum == "Aktif").ToListAsync();

            var kullanicilar = await _context.Kullanicilars
                .Where(u => !u.SilindiMi)
                .Select(u => new KullaniciListeViewModel
                {
                    Id = u.Id,
                    Ad = u.Ad,
                    Soyad = u.Soyad,
                    KullaniciAdi = u.KullaniciAdi,
                    Eposta = u.Eposta,
                    Telefon = u.Telefon,
                    Durum = u.Durum,
                    SonGiris = u.SonGirisTarihi,
                    Roller = _context.KullaniciRolleris
                        .Where(kr => kr.KullaniciId == u.Id)
                        .Join(_context.Rollers, kr => kr.RolId, r => r.Id, (kr, r) => r.RolAdi)
                        .ToList()
                })
                .OrderByDescending(u => u.Id)
                .ToListAsync();

            return View(kullanicilar);
        }

        // --- 3. KULLANICI DETAY (DÜZENLEME İÇİN) ---
        [HttpGet]
        public async Task<IActionResult> KullaniciDetay(int id)
        {
            var user = await _context.Kullanicilars
                .Where(u => u.Id == id)
                .Select(u => new {
                    u.Id,
                    u.Ad,
                    u.Soyad,
                    u.KullaniciAdi,
                    u.Eposta,
                    u.Telefon,
                    u.Durum,
                    u.IsOnayli,
                    u.SifreHash,
                    RolId = _context.KullaniciRolleris.Where(kr => kr.KullaniciId == u.Id).Select(kr => kr.RolId).FirstOrDefault()
                }).FirstOrDefaultAsync();

            if (user == null) return NotFound();

            return Json(user);
        }

        // --- 4. KULLANICI KAYDET / GÜNCELLE ---
        [HttpPost]
        public async Task<IActionResult> KullaniciKaydet(Kullanicilar model, string? SifreInput, int SecilenRolId)
        {
            try
            {
                if (model.Id == 0) // YENİ KAYIT
                {
                    if (string.IsNullOrEmpty(SifreInput))
                        return Json(new { success = false, message = "Yeni kullanıcı için şifre zorunludur!" });

                    model.SifreHash = SifreleSHA256(SifreInput);
                    model.OlusturmaTarihi = DateTime.Now;
                    model.SilindiMi = false;

                    _context.Kullanicilars.Add(model);
                    await _context.SaveChangesAsync();

                    // Seçilen rolü ilişkilendir
                    _context.KullaniciRolleris.Add(new KullaniciRolleri
                    {
                        KullaniciId = model.Id,
                        RolId = SecilenRolId,
                        AtanmaTarihi = DateTime.Now
                    });
                }
                else // GÜNCELLEME
                {
                    var user = await _context.Kullanicilars.FindAsync(model.Id);
                    if (user == null) return Json(new { success = false, message = "Kullanıcı bulunamadı!" });

                    user.Ad = model.Ad;
                    user.Soyad = model.Soyad;
                    user.KullaniciAdi = model.KullaniciAdi;
                    user.Eposta = model.Eposta;
                    user.Telefon = model.Telefon;
                    user.Durum = model.Durum;
                    user.IsOnayli = model.IsOnayli;

                    // Şifre kutusu doluysa güncelle
                    if (!string.IsNullOrEmpty(SifreInput) && SifreInput.Length < 32)
                    {
                        user.SifreHash = SifreleSHA256(SifreInput);
                    }

                    // Mevcut rolü güncelle veya yeni ekle
                    var mevcutRol = await _context.KullaniciRolleris.FirstOrDefaultAsync(kr => kr.KullaniciId == user.Id);
                    if (mevcutRol != null)
                        mevcutRol.RolId = SecilenRolId;
                    else
                        _context.KullaniciRolleris.Add(new KullaniciRolleri { KullaniciId = user.Id, RolId = SecilenRolId, AtanmaTarihi = DateTime.Now });

                    _context.Kullanicilars.Update(user);
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }

        // --- 5. KULLANICI SİL ---
        [HttpPost]
        public async Task<IActionResult> KullaniciSil(int id)
        {
            var user = await _context.Kullanicilars.FindAsync(id);
            if (user != null)
            {
                user.SilindiMi = true;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Kullanıcı bulunamadı." });
        }

        // --- 6. PROFİL GÖRÜNTÜLE ---
        [HttpGet]
        public async Task<IActionResult> Profil()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var user = await _context.Kullanicilars.FindAsync(userId);
            if (user == null) return NotFound();

            var rolId = await _context.KullaniciRolleris
                .Where(kr => kr.KullaniciId == userId)
                .Select(kr => kr.RolId)
                .FirstOrDefaultAsync();

            ViewBag.RolAdi = await _context.Rollers
                .Where(r => r.Id == rolId)
                .Select(r => r.RolAdi)
                .FirstOrDefaultAsync() ?? "Kullanıcı";

            return View(user);
        }

        // --- 7. PROFİL GÜNCELLE ---
        [HttpPost]
        public async Task<IActionResult> ProfilGuncelle(Kullanicilar model, string? YeniSifre)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var user = await _context.Kullanicilars.FindAsync(userId);

            if (user != null)
            {
                user.Ad = model.Ad;
                user.Soyad = model.Soyad;
                user.Eposta = model.Eposta;
                user.Telefon = model.Telefon;

                if (!string.IsNullOrEmpty(YeniSifre))
                    user.SifreHash = SifreleSHA256(YeniSifre);

                _context.Kullanicilars.Update(user);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetString("FullName", user.Ad + " " + user.Soyad);
                return Json(new { success = true, message = "Profil başarıyla güncellendi." });
            }
            return Json(new { success = false });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}