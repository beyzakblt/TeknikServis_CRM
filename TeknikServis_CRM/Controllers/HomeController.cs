#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TeknikServis_CRM.Models;
using TeknikServis_CRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        // --- 1. DASHBOARD ---
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var fullName = HttpContext.Session.GetString("FullName");
            ViewBag.UserFullName = !string.IsNullOrEmpty(fullName) ? fullName : (User.Identity?.Name ?? "Kullanıcı");

            var userIdStr = User.FindFirst("UserId")?.Value;
            ViewBag.UserRole = "Yetkili";
            if (int.TryParse(userIdStr, out int userId))
            {
                var rol = await _context.KullaniciRolleris
                    .Where(kr => kr.KullaniciId == userId)
                    .Join(_context.Rollers, kr => kr.RolId, r => r.Id, (kr, r) => r.RolAdi)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(rol)) ViewBag.UserRole = rol;
            }

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
            ViewBag.ToplamMusteri = await _context.Musterilers.CountAsync(x => !x.SilindiMi);
            ViewBag.AktifIsEmri = await _context.IsEmirleris.CountAsync(x => x.Durum != "Tamamlandı" && x.Durum != "İptal");

            // =========================================================
            // BUGÜNKÜ RANDEVULAR (JSON PAKETİNİ AÇMA İŞLEMİ)
            // =========================================================
            var bugun = DateTime.Today;
            var bugunkuRandevularRaw = await _context.Randevulars
                .Include(x => x.Musteri)
                .Where(x => x.RandevuTarihi.HasValue && x.RandevuTarihi.Value.Date == bugun)
                .OrderBy(x => x.RandevuTarihi)
                .ToListAsync();

            var randevuListesi = new List<dynamic>();
            foreach (var r in bugunkuRandevularRaw)
            {
                string gercekBaslik = "Randevu"; // Varsayılan

                // Gizli JSON paketini açıp içinden "Baslik" alanını çekiyoruz
                if (!string.IsNullOrEmpty(r.Aciklama) && r.Aciklama.StartsWith("{"))
                {
                    try
                    {
                        using var doc = System.Text.Json.JsonDocument.Parse(r.Aciklama);
                        if (doc.RootElement.TryGetProperty("Baslik", out var baslikProp))
                        {
                            gercekBaslik = baslikProp.GetString();
                        }
                    }
                    catch { }
                }

                randevuListesi.Add(new
                {
                    Saat = r.RandevuTarihi?.ToString("HH:mm"),
                    MusteriAd = r.Musteri?.AdSoyad ?? "Bilinmiyor",
                    Konu = gercekBaslik,
                    MusteriId = r.MusteriId
                });
            }

            ViewBag.BugunRandevuSayisi = randevuListesi.Count;
            ViewBag.BugunRandevuListesi = randevuListesi;

            // SON 5 İŞ EMRİ
            ViewBag.SonIsEmirleri = await _context.IsEmirleris
                .Include(x => x.Musteri)
                .OrderByDescending(x => x.Id)
                .Take(5)
                .ToListAsync();

            return View();
        }

        // --- 2. KULLANICILARI LİSTELE (MODAL İÇİN ROLLER DAHİL) ---
        [HttpGet]
        public async Task<IActionResult> Kullanicilar()
        {
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
        public async Task<IActionResult> KullaniciKaydet(Kullanicilar model, string SifreInput, int SecilenRolId)
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

                    if (!string.IsNullOrEmpty(SifreInput) && SifreInput.Length < 32)
                    {
                        user.SifreHash = SifreleSHA256(SifreInput);
                    }

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
        public async Task<IActionResult> ProfilGuncelle(Kullanicilar model, string YeniSifre)
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
        [HttpGet]
        public async Task<JsonResult> GetNotifications()
        {
            var bugun = DateTime.Today;
            var ucGunSonra = bugun.AddDays(3);

            // 1. Ödemesi Yaklaşanlar (KapanisTarihi'ni vade olarak kullanıyoruz)
            var odemeler = await _context.ServisKayitlaris
                .Include(x => x.Musteri)
                .Where(x => x.OdemeDurumu == "Bekliyor" && x.KapanisTarihi >= bugun && x.KapanisTarihi <= ucGunSonra)
                .OrderBy(x => x.KapanisTarihi)
                .Select(s => new {
                    Baslik = "Ödeme Yaklaştı",
                    Detay = s.Musteri.AdSoyad + " (" + s.ServisNo + ")",
                    Tip = "payment",
                    Tarih = s.KapanisTarihi.Value.ToString("dd.MM")
                })
                .ToListAsync();

            // 2. Yeni Atanan İş Emirleri
            var servisler = await _context.IsEmirleris
                .Where(x => x.Durum == "Yeni Atandı")
                .OrderByDescending(x => x.OlusturmaTarihi)
                .Select(s => new {
                    Baslik = "Yeni İş Emri",
                    Detay = "Atanmış bekleyen iş emri mevcut.",
                    Tip = "service",
                    Tarih = s.OlusturmaTarihi.Value.ToString("HH:mm")
                })
                .ToListAsync();

            var items = odemeler.Concat(servisler).ToList();

            return Json(new
            {
                count = items.Count,
                items = items.Take(10) // Son 10 bildirim 
            });
        }
        [HttpGet]
        public async Task<IActionResult> PersonelIsYuku(int id)
        {
            // Verilen kullanıcı ID'sine (TeknisyenId) atanmış olan iş emirlerini getiriyoruz
            var isYuku = await _context.IsEmirleris
                .Where(ie => ie.TeknisyenId == id)
                .Include(ie => ie.Musteri)
                .Select(ie => new {
                    ie.Id,
                    MusteriAd = ie.Musteri.AdSoyad,
                    ie.ArizaAciklamasi,
                    ie.Durum,
                    Tarih = ie.OlusturmaTarihi.HasValue ? ie.OlusturmaTarihi.Value.ToString("dd.MM.yyyy HH:mm") : "-",
                    ServisUcreti = ie.ServisUcreti ?? 0
                })
                .OrderByDescending(ie => ie.Id)
                .ToListAsync();

            return Json(isYuku);
        }
    }

    // IP-API Servisi için sınıf
    public class IpLocationResult
    {
        public string status { get; set; }
        public string city { get; set; }
        public string regionName { get; set; }
        public string country { get; set; }
    }
}