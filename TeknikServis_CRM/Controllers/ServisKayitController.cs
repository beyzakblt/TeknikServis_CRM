#nullable disable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeknikServis_CRM.Models;

namespace TeknikServis_CRM.Controllers
{
    [Authorize]
    public class ServisKayitController : Controller
    {
        private readonly CrmDbContext _context;
        public ServisKayitController(CrmDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            ViewBag.Musteriler = await _context.Musterilers.AsNoTracking().Where(x => !x.SilindiMi).OrderBy(x => x.AdSoyad).ToListAsync();
            ViewBag.Durumlar = await _context.ServisDurumlaris.AsNoTracking().OrderBy(x => x.Id).ToListAsync();
            ViewBag.Teknisyenler = await (from k in _context.Kullanicilars
                                          join kr in _context.KullaniciRolleris on k.Id equals kr.KullaniciId
                                          join r in _context.Rollers on kr.RolId equals r.Id
                                          where r.RolKodu == "TEK" || r.RolAdi.Contains("Teknisyen")
                                          select new { k.Id, AdSoyad = k.Ad + " " + k.Soyad }).Distinct().ToListAsync();
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetServisListesi()
        {
            var liste = await (from s in _context.ServisKayitlaris
                               join m in _context.Musterilers on s.MusteriId equals m.Id into mj
                               from m in mj.DefaultIfEmpty()
                               join d in _context.ServisDurumlaris on s.ServisDurumId equals d.Id into dj
                               from d in dj.DefaultIfEmpty()
                               join c in _context.MusteriCihazlaris on s.MusteriCihazId equals c.Id into cj
                               from c in cj.DefaultIfEmpty()
                               let isEmri = _context.IsEmirleris.FirstOrDefault(ie => ie.MusteriId == s.MusteriId && ie.CihazId == s.MusteriCihazId)
                               let teknisyen = _context.Kullanicilars.FirstOrDefault(k => k.Id == isEmri.TeknisyenId)
                               orderby s.Id descending
                               select new
                               {
                                   s.Id,
                                   s.ServisNo,
                                   MusteriAd = m != null ? m.AdSoyad : "Bilinmeyen Müşteri",
                                   CihazBilgi = c != null ? c.Marka + " " + c.Model : "Cihaz Belirtilmemiş",
                                   DurumAdi = isEmri != null ? isEmri.Durum : (d != null ? d.DurumAdi : "Durum Atanmadı"),
                                   RenkKodu = d != null ? d.RenkKodu : "#cccccc",
                                   s.ArizaAciklamasi,
                                   s.Ucret,
                                   s.OdemeDurumu,
                                   OdemeTarihi = s.KapanisTarihi,
                                   ServisTarihi = s.ServisTarihi,
                                   TeknisyenAd = teknisyen != null ? teknisyen.Ad + " " + teknisyen.Soyad : "Atanmadı"
                               }).AsNoTracking().ToListAsync();

            return Json(liste);
        }

        [HttpGet]
        public async Task<JsonResult> GetYaklasanOdemeler()
        {
            var bugun = DateTime.Today;
            var ucGunSonra = bugun.AddDays(3);

            var liste = await _context.ServisKayitlaris
                .AsNoTracking()
                .Include(x => x.Musteri)
                .Where(x => x.OdemeDurumu == "Bekliyor" && x.KapanisTarihi.HasValue && x.KapanisTarihi >= bugun && x.KapanisTarihi <= ucGunSonra)
                .Select(s => new {
                    s.ServisNo,
                    MusteriAd = s.Musteri.AdSoyad,
                    s.Ucret,
                    Tarih = s.KapanisTarihi.Value.ToString("dd.MM.yyyy")
                }).ToListAsync();

            return Json(liste);
        }

        [HttpPost]
        public async Task<JsonResult> Kaydet(ServisKayitlari model, int? TeknisyenId, string TeknikNot, string Durum, DateTime? VadeTarihi)
        {
            try
            {
                if (model.Id == 0)
                {
                    var sonKayit = await _context.ServisKayitlaris.OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                    model.ServisNo = "SRV-" + DateTime.Now.Year + "-" + ((sonKayit?.Id ?? 0) + 1001);
                    model.ServisTarihi = DateTime.Now;
                    model.KapanisTarihi = VadeTarihi;
                    _context.ServisKayitlaris.Add(model);
                }
                else
                {
                    var ent = await _context.ServisKayitlaris.FindAsync(model.Id);
                    if (ent == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

                    ent.MusteriId = model.MusteriId;
                    ent.MusteriCihazId = model.MusteriCihazId;
                    ent.ServisDurumId = model.ServisDurumId;
                    ent.ArizaAciklamasi = model.ArizaAciklamasi;
                    ent.Ucret = model.Ucret;
                    ent.OdemeDurumu = model.OdemeDurumu;
                    ent.KapanisTarihi = VadeTarihi;
                }

                await _context.SaveChangesAsync();

                var isEmri = await _context.IsEmirleris.FirstOrDefaultAsync(x => x.MusteriId == (int)model.MusteriId && x.CihazId == model.MusteriCihazId);

                if (isEmri == null)
                {
                    _context.IsEmirleris.Add(new IsEmirleri
                    {
                        MusteriId = (int)model.MusteriId,
                        CihazId = model.MusteriCihazId,
                        TeknisyenId = TeknisyenId,
                        ArizaAciklamasi = model.ArizaAciklamasi,
                        TeknikNot = TeknikNot,
                        Durum = Durum ?? "Yeni Atandı",
                        OlusturmaTarihi = DateTime.Now,
                        ServisUcreti = model.Ucret
                    });
                }
                else
                {
                    isEmri.TeknisyenId = TeknisyenId;
                    isEmri.TeknikNot = TeknikNot;
                    isEmri.Durum = Durum;
                    isEmri.ArizaAciklamasi = model.ArizaAciklamasi;
                    isEmri.ServisUcreti = model.Ucret;
                }

                await _context.SaveChangesAsync();

                if (model.OdemeDurumu == "Ödendi" && (model.Ucret ?? 0) > 0)
                {
                    var bakiyeIsliMi = await _context.TahsilatHareketleris.AnyAsync(x => x.ServisKaydiId == model.Id);
                    if (!bakiyeIsliMi)
                    {
                        int mId = (int)model.MusteriId;
                        decimal tutar = model.Ucret ?? 0;
                        _context.TahsilatHareketleris.Add(new TahsilatHareketleri { MusteriId = mId, ServisKaydiId = model.Id, HareketTipi = "Giriş", Tutar = tutar, OdemeYontemi = "Nakit", IslemTarihi = DateTime.Now });
                        _context.KasaHareketleris.Add(new KasaHareketleri { MusteriId = mId, Tutar = tutar, IslemTipi = "Tahsilat", OdemeYontemi = "Nakit", Tarih = DateTime.Now });
                        await _context.SaveChangesAsync();
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = "Hata: " + (ex.InnerException?.Message ?? ex.Message) }); }
        }

        [HttpGet]
        public async Task<JsonResult> GetMusteriCihazlari(int musteriId)
        {
            if (musteriId == 0) return Json(new List<object>());
            return Json(await _context.MusteriCihazlaris.AsNoTracking().Where(x => x.MusteriId == musteriId && x.AktifMi == true).Select(x => new { x.Id, ad = x.Marka + " " + x.Model }).ToListAsync());
        }

        [HttpGet]
        public async Task<JsonResult> GetIsEmri(int servisId)
        {
            var servis = await _context.ServisKayitlaris.FindAsync(servisId);
            if (servis == null) return Json(null);
            return Json(await _context.IsEmirleris.AsNoTracking().FirstOrDefaultAsync(x => x.MusteriId == servis.MusteriId && x.CihazId == servis.MusteriCihazId));
        }

        [HttpGet]
        public async Task<JsonResult> GetServis(int id) => Json(await _context.ServisKayitlaris.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id));

        [HttpPost]
        public async Task<JsonResult> Sil(int id)
        {
            var s = await _context.ServisKayitlaris.FindAsync(id);
            if (s != null) { _context.ServisKayitlaris.Remove(s); await _context.SaveChangesAsync(); }
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> ServisFisiIndir(int id)
        {
            var servis = await _context.ServisKayitlaris.Include(x => x.Musteri).Include(x => x.MusteriCihaz).FirstOrDefaultAsync(x => x.Id == id);
            if (servis == null) return NotFound();

            var document = Document.Create(container => {
                container.Page(page => {
                    page.Margin(1, Unit.Centimetre); page.Size(PageSizes.A4); page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));
                    page.Header().Row(row => {
                        row.RelativeItem().Column(col => { col.Item().Text("TEKNİK SERVİS CRM").FontSize(20).SemiBold().FontColor(Colors.Blue.Medium); col.Item().Text("Profesyonel Teknik Destek Çözümleri"); });
                        row.RelativeItem().AlignRight().Column(col => { col.Item().Text($"Servis No: {servis.ServisNo}").FontSize(12).SemiBold(); col.Item().Text($"Tarih: {servis.ServisTarihi?.ToString("dd.MM.yyyy HH:mm")}"); });
                    });
                    page.Content().PaddingVertical(10).Column(col => {
                        col.Spacing(10);
                        col.Item().Table(table => {
                            table.ColumnsDefinition(columns => { columns.RelativeColumn(); columns.RelativeColumn(); });
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Müşteri Bilgileri").SemiBold();
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Cihaz Bilgileri").SemiBold();
                            table.Cell().Border(0.5f).Padding(5).Text($"{servis.Musteri?.AdSoyad}\nTel: {servis.Musteri?.Telefon}");
                            table.Cell().Border(0.5f).Padding(5).Text($"{servis.MusteriCihaz?.Marka} {servis.MusteriCihaz?.Model}\nSeri No: {servis.MusteriCihaz?.SeriNo}");
                        });
                        col.Item().Text("Arıza Açıklaması:").SemiBold(); col.Item().PaddingLeft(5).Text(servis.ArizaAciklamasi ?? "Belirtilmedi"); col.Item().LineHorizontal(0.5f);
                        col.Item().Text("Yapılan İşlem:").SemiBold(); col.Item().PaddingLeft(5).Text(servis.YapilanIslem ?? "İşlem devam ediyor...");
                        col.Item().AlignRight().PaddingTop(10).Text($"Toplam Tutar: {servis.Ucret?.ToString("C2") ?? "0,00 TL"}").FontSize(14).Bold();
                    });
                    page.Footer().Row(row => {
                        row.RelativeItem().Column(col => { col.Item().PaddingTop(10).Text("Müşteri İmzası"); col.Item().PaddingTop(20).Text("....................."); });
                        row.RelativeItem().AlignRight().Column(col => { col.Item().PaddingTop(10).Text("Servis Yetkilisi"); col.Item().PaddingTop(20).Text("....................."); });
                    });
                });
            });

            return File(document.GeneratePdf(), "application/pdf", $"ServisFisi_{servis.ServisNo}.pdf");
        }
    }
}