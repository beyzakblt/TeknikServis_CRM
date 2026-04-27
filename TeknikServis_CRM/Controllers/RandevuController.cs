#nullable disable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TeknikServis_CRM.Models;

namespace TeknikServis_CRM.Controllers
{
    [Authorize]
    public class RandevuController : Controller
    {
        private readonly CrmDbContext _context;
        public RandevuController(CrmDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            ViewBag.Musteriler = await _context.Musterilers
                .Where(x => !x.SilindiMi)
                .OrderBy(x => x.AdSoyad)
                .ToListAsync();
            return View();
        }

        // Açıklama kolonuna saklayacağımız gizli paketin şablonu
        private class RandevuEkstraData
        {
            public string Baslik { get; set; }
            public string Durum { get; set; }
            public string Renk { get; set; }
            public string BitisTarihi { get; set; }
            public string GercekNot { get; set; }
        }

        [HttpGet]
        public async Task<JsonResult> GetRandevular()
        {
            // 1. Manuel Oluşturulan Randevuları Getir
            var rList = await _context.Randevulars.AsNoTracking().Include(r => r.Musteri).ToListAsync();

            var randevuEvents = rList.Select(r => {
                string baslik = r.Musteri != null ? r.Musteri.AdSoyad : "Randevu";
                string durum = "Bekliyor";
                string renk = "#4e73df";
                DateTime bitis = r.RandevuTarihi?.AddHours(1) ?? DateTime.Now;
                string aciklamaNotu = r.Aciklama;

                if (!string.IsNullOrEmpty(r.Aciklama) && r.Aciklama.StartsWith("{"))
                {
                    try
                    {
                        var ekstra = JsonSerializer.Deserialize<RandevuEkstraData>(r.Aciklama);
                        if (ekstra != null)
                        {
                            baslik = ekstra.Baslik ?? baslik;
                            durum = ekstra.Durum ?? durum;
                            renk = ekstra.Renk ?? renk;
                            aciklamaNotu = ekstra.GercekNot;
                            if (DateTime.TryParse(ekstra.BitisTarihi, out DateTime parsedBitis)) bitis = parsedBitis;
                        }
                    }
                    catch { }
                }

                return new
                {
                    id = "R_" + r.Id, // ID çakışmaması için ön ek
                    title = "[RAND] " + baslik,
                    start = r.RandevuTarihi?.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = bitis.ToString("yyyy-MM-ddTHH:mm:ss"),
                    color = renk,
                    description = aciklamaNotu,
                    allDay = false
                };
            });

            // 2. İş Emirlerini Getir (Servis Kayıtlarıyla Birlikte)
            // ServisKayitlaris tablosundaki KapanisTarihi'ni "İş Emri Vadesi" olarak kullanıyoruz
            var isEmriList = await _context.IsEmirleris
                .AsNoTracking()
                .Include(ie => ie.Musteri)
                .Where(ie => ie.Durum != "Tamamlandı" && ie.Durum != "İptal")
                .ToListAsync();

            var isEmriEvents = isEmriList.Select(ie => {
                // Servis kaydındaki vade tarihini bulalım
                var servis = _context.ServisKayitlaris.FirstOrDefault(s => s.MusteriId == ie.MusteriId && s.MusteriCihazId == ie.CihazId);
                DateTime? baslangic = servis?.KapanisTarihi ?? ie.OlusturmaTarihi;

                return new
                {
                    id = "IE_" + ie.Id,
                    title = "[İŞ] " + (ie.Musteri?.AdSoyad ?? "İş Emri"),
                    start = baslangic?.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = baslangic?.AddHours(2).ToString("yyyy-MM-ddTHH:mm:ss"),
                    color = "#f6c23e", // İş emirleri için standart sarı renk
                    description = ie.ArizaAciklamasi,
                    allDay = false
                };
            });

            // İki listeyi birleştir ve geri döndür
            var totalEvents = randevuEvents.Concat(isEmriEvents).ToList();
            return Json(totalEvents);
        }

        [HttpPost]
        public async Task<JsonResult> Kaydet(int Id, int? MusteriId, DateTime? RandevuTarihi, DateTime? BitisTarihi, string Baslik, string Durum, string RenkKodu, string Aciklama)
        {
            try
            {
                // Formdan gelen bilgileri bir JSON paketine dönüştürüyoruz
                var ekstraVeri = new RandevuEkstraData
                {
                    Baslik = Baslik,
                    Durum = Durum,
                    Renk = RenkKodu,
                    BitisTarihi = BitisTarihi?.ToString("yyyy-MM-ddTHH:mm:ss"),
                    GercekNot = Aciklama
                };

                // Paketi metin haline getir (String yap)
                string gizliPaket = JsonSerializer.Serialize(ekstraVeri);

                if (Id == 0)
                {
                    var yeniRandevu = new Randevular
                    {
                        MusteriId = MusteriId,
                        RandevuTarihi = RandevuTarihi,
                        Aciklama = gizliPaket // Tüm bilgileri tek bir string olarak Aciklama'ya basıyoruz!
                    };
                    _context.Randevulars.Add(yeniRandevu);
                }
                else
                {
                    var ent = await _context.Randevulars.FindAsync(Id);
                    if (ent == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

                    ent.MusteriId = MusteriId;
                    ent.RandevuTarihi = RandevuTarihi;
                    ent.Aciklama = gizliPaket;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpGet]
        public async Task<JsonResult> GetRandevu(int id)
        {
            var r = await _context.Randevulars.FindAsync(id);
            if (r == null) return Json(null);

            // Veriyi ekrana yollarken paketi açıp form alanlarına uygun hale getiriyoruz
            var response = new
            {
                id = r.Id,
                musteriId = r.MusteriId,
                randevuTarihi = r.RandevuTarihi,
                baslik = "",
                aciklama = r.Aciklama,
                bitisTarihi = "",
                durum = "Bekliyor",
                renkKodu = "#4e73df"
            };

            if (!string.IsNullOrEmpty(r.Aciklama) && r.Aciklama.StartsWith("{") && r.Aciklama.EndsWith("}"))
            {
                try
                {
                    var ekstra = JsonSerializer.Deserialize<RandevuEkstraData>(r.Aciklama);
                    if (ekstra != null)
                    {
                        response = new
                        {
                            id = r.Id,
                            musteriId = r.MusteriId,
                            randevuTarihi = r.RandevuTarihi,
                            baslik = ekstra.Baslik,
                            aciklama = ekstra.GercekNot,
                            bitisTarihi = ekstra.BitisTarihi,
                            durum = ekstra.Durum,
                            renkKodu = ekstra.Renk
                        };
                    }
                }
                catch { }
            }

            return Json(response);
        }

        [HttpPost]
        public async Task<JsonResult> Sil(int id)
        {
            var r = await _context.Randevulars.FindAsync(id);
            if (r != null) { _context.Randevulars.Remove(r); await _context.SaveChangesAsync(); }
            return Json(new { success = true });
        }
    }
}