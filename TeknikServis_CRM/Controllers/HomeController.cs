using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TeknikServis_CRM.Models;
using TeknikServis_CRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace TeknikServis_CRM.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CrmDbContext _context;

        public HomeController(ILogger<HomeController> logger, CrmDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public static string SifreleSHA256(string hamSifre)
        {
            if (string.IsNullOrEmpty(hamSifre)) return "";
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(hamSifre));
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var fullName = HttpContext.Session.GetString("FullName");
            ViewBag.UserFullName = !string.IsNullOrEmpty(fullName) ? fullName : (User.Identity?.Name ?? "Kullanıcı");

            var userIdClaim = User.FindFirst("UserId")?.Value;
            var today = DateTime.Today;

            // 1. KULLANICI ROLÜ TESPİTİ
            ViewBag.UserRole = "Yetkili";
            if (int.TryParse(userIdClaim, out int userId))
            {
                var rol = await _context.KullaniciRolleris
                    .Where(kr => kr.KullaniciId == userId)
                    .Join(_context.Rollers, kr => kr.RolId, r => r.Id, (kr, r) => r.RolAdi)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(rol)) ViewBag.UserRole = rol;
            }

            // 2. DASHBOARD KPI KARTLARI (DİNAMİK)
            ViewBag.TotalCihaz = await _context.MusteriCihazlaris.CountAsync(x => x.AktifMi == true);
            ViewBag.TotalUsers = await _context.Kullanicilars.CountAsync(x => !x.SilindiMi);
            ViewBag.ToplamMusteri = await _context.Musterilers.CountAsync(x => !x.SilindiMi);
            ViewBag.AktifIsEmri = await _context.IsEmirleris.CountAsync(x => x.Durum != "Tamamlandı" && x.Durum != "İptal");

            // Bu ayın toplam tahsilatı
            ViewBag.AylikTahsilat = await _context.KasaHareketleris
                .Where(x => x.Tarih.Value.Month == DateTime.Now.Month && x.Tarih.Value.Year == DateTime.Now.Year && x.IslemTipi == "Tahsilat")
                .SumAsync(x => (decimal?)x.Tutar) ?? 0;

            // 3. BUGÜNÜN AJANDASI (JSON PAKET AÇMALI)
            var bugunkuRandevularRaw = await _context.Randevulars
                .Include(x => x.Musteri)
                .Where(x => x.RandevuTarihi.HasValue && x.RandevuTarihi.Value.Date == today)
                .OrderBy(x => x.RandevuTarihi)
                .ToListAsync();

            var randevuListesi = new List<object>();
            foreach (var r in bugunkuRandevularRaw)
            {
                string gercekBaslik = "Randevu";
                if (!string.IsNullOrEmpty(r.Aciklama) && r.Aciklama.StartsWith("{"))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(r.Aciklama);
                        if (doc.RootElement.TryGetProperty("Baslik", out var baslikProp))
                            gercekBaslik = baslikProp.GetString();
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

            // 4. SON İŞ EMİRLERİ LİSTESİ
            ViewBag.SonIsEmirleri = await _context.IsEmirleris
                .Include(x => x.Musteri)
                .OrderByDescending(x => x.Id)
                .Take(5)
                .ToListAsync();

            // 5. GRAFİK VERİLERİ (DİNAMİK)
            // Son 6 ayın servis yoğunluğu
            var grafikData = new List<int>();
            for (int i = 5; i >= 0; i--)
            {
                var hedefAy = DateTime.Now.AddMonths(-i);
                var count = await _context.ServisKayitlaris
                    .CountAsync(x => x.ServisTarihi.Value.Month == hedefAy.Month && x.ServisTarihi.Value.Year == hedefAy.Year);
                grafikData.Add(count);
            }
            ViewBag.ServisGrafikData = JsonSerializer.Serialize(grafikData);

            // İş Emri Durum Dağılımı (Pie Chart)
            var pastaData = new List<int>
            {
                await _context.IsEmirleris.CountAsync(x => x.Durum == "Tamamlandı"),
                await _context.IsEmirleris.CountAsync(x => x.Durum == "İşlemde"),
                await _context.IsEmirleris.CountAsync(x => x.Durum == "Parça Bekliyor")
            };
            ViewBag.PastaGrafikData = JsonSerializer.Serialize(pastaData);

            return View();
        }

        public IActionResult Bildirim() => View();

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

        [HttpGet]
        public async Task<IActionResult> KullaniciDetay(int id)
        {
            var user = await _context.Kullanicilars
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.Ad,
                    u.Soyad,
                    u.KullaniciAdi,
                    u.Eposta,
                    u.Telefon,
                    u.Durum,
                    u.IsOnayli,
                    RolId = _context.KullaniciRolleris.Where(kr => kr.KullaniciId == u.Id).Select(kr => kr.RolId).FirstOrDefault()
                }).FirstOrDefaultAsync();

            if (user == null) return NotFound();
            return Json(user);
        }

        [HttpPost]
        public async Task<IActionResult> KullaniciKaydet(Kullanicilar model, string SifreInput, int SecilenRolId)
        {
            try
            {
                if (model.Id == 0)
                {
                    if (string.IsNullOrEmpty(SifreInput)) return Json(new { success = false, message = "Şifre zorunludur!" });
                    model.SifreHash = SifreleSHA256(SifreInput);
                    model.OlusturmaTarihi = DateTime.Now;
                    model.SilindiMi = false;
                    _context.Kullanicilars.Add(model);
                    await _context.SaveChangesAsync();
                    _context.KullaniciRolleris.Add(new KullaniciRolleri { KullaniciId = model.Id, RolId = SecilenRolId, AtanmaTarihi = DateTime.Now });
                }
                else
                {
                    var user = await _context.Kullanicilars.FindAsync(model.Id);
                    if (user == null) return Json(new { success = false, message = "Kullanıcı bulunamadı!" });
                    user.Ad = model.Ad; user.Soyad = model.Soyad; user.KullaniciAdi = model.KullaniciAdi;
                    user.Eposta = model.Eposta; user.Telefon = model.Telefon; user.Durum = model.Durum; user.IsOnayli = model.IsOnayli;
                    if (!string.IsNullOrEmpty(SifreInput)) user.SifreHash = SifreleSHA256(SifreInput);

                    var mevcutRol = await _context.KullaniciRolleris.FirstOrDefaultAsync(kr => kr.KullaniciId == user.Id);
                    if (mevcutRol != null) mevcutRol.RolId = SecilenRolId;
                    else _context.KullaniciRolleris.Add(new KullaniciRolleri { KullaniciId = user.Id, RolId = SecilenRolId, AtanmaTarihi = DateTime.Now });
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpPost]
        public async Task<IActionResult> KullaniciSil(int id)
        {
            var user = await _context.Kullanicilars.FindAsync(id);
            if (user != null) { user.SilindiMi = true; await _context.SaveChangesAsync(); return Json(new { success = true }); }
            return Json(new { success = false, message = "Kullanıcı bulunamadı." });
        }

        [HttpGet]
        public async Task<IActionResult> Profil()
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdStr, out int userIdVal)) return Unauthorized();

            var user = await _context.Kullanicilars.FindAsync(userIdVal);
            if (user == null) return NotFound();

            var rolId = await _context.KullaniciRolleris.Where(kr => kr.KullaniciId == userIdVal).Select(kr => kr.RolId).FirstOrDefaultAsync();
            ViewBag.RolAdi = await _context.Rollers.Where(r => r.Id == rolId).Select(r => r.RolAdi).FirstOrDefaultAsync() ?? "Kullanıcı";

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> ProfilGuncelle(Kullanicilar model, string YeniSifre)
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdStr, out int userIdVal)) return Unauthorized();

            var user = await _context.Kullanicilars.FindAsync(userIdVal);
            if (user != null)
            {
                user.Ad = model.Ad; user.Soyad = model.Soyad; user.Eposta = model.Eposta; user.Telefon = model.Telefon;
                if (!string.IsNullOrEmpty(YeniSifre)) user.SifreHash = SifreleSHA256(YeniSifre);
                _context.Kullanicilars.Update(user);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("FullName", user.Ad + " " + user.Soyad);
                return Json(new { success = true, message = "Profil güncellendi." });
            }
            return Json(new { success = false });
        }
        [HttpGet]
        public async Task<JsonResult> GetNotifications()
        {
            var bugun = DateTime.Today;
            var ucGunSonra = bugun.AddDays(3);
            var birHaftaOnce = bugun.AddDays(-7);

            // 1. Ödemeler
            var odemeler = await _context.ServisKayitlaris
                .Include(x => x.Musteri)
                .Where(x => x.OdemeDurumu == "Bekliyor" && x.KapanisTarihi >= birHaftaOnce && x.KapanisTarihi <= ucGunSonra)
                .Select(s => new {
                    Baslik = "Ödeme Vadesi",
                    Detay = s.Musteri.AdSoyad,
                    Tip = "payment",
                    TarihVal = s.KapanisTarihi,
                    Tarih = s.KapanisTarihi.Value.ToString("dd.MM"),
                    Url = "/ServisKayit/Index",
                    Urgency = s.KapanisTarihi < bugun ? "high" : "normal"
                }).ToListAsync();

            // 2. Yeni İş Emirleri
            var servisler = await _context.IsEmirleris
                .Include(x => x.Musteri)
                .Where(x => x.OlusturmaTarihi >= birHaftaOnce)
                .Select(s => new {
                    Baslik = "İş Emri Güncellemesi",
                    Detay = s.Musteri.AdSoyad + " - " + s.Durum,
                    Tip = "service",
                    TarihVal = s.OlusturmaTarihi,
                    Tarih = s.OlusturmaTarihi.Value.ToString("dd.MM HH:mm"),
                    Url = "/ServisKayit/Index",
                    Urgency = s.Durum == "Yeni Atandı" ? "normal" : "low"
                }).ToListAsync();

            // 3. Randevular
            var randevular = await _context.Randevulars
                .Include(x => x.Musteri)
                .Where(x => x.RandevuTarihi >= birHaftaOnce && x.RandevuTarihi <= ucGunSonra)
                .Select(r => new {
                    Baslik = "Randevu",
                    Detay = r.Musteri.AdSoyad,
                    Tip = "appointment",
                    TarihVal = r.RandevuTarihi,
                    Tarih = r.RandevuTarihi.Value.ToString("dd.MM HH:mm"),
                    Url = "/Randevu/Index",
                    Urgency = "low"
                }).ToListAsync();

            var allItems = odemeler.Cast<object>()
                .Concat(servisler.Cast<object>())
                .Concat(randevular.Cast<object>())
                .ToList();

            // Verileri tarihe göre sıralayıp JS tarafına gönderiyoruz
            return Json(new { items = allItems });
        }

        [HttpGet]
        public async Task<IActionResult> PersonelIsYuku(int id)
        {
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
                }).ToListAsync();
            return Json(isYuku);
        }
    }
}