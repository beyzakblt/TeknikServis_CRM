#nullable disable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis_CRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using ClosedXML.Excel;

namespace TeknikServis_CRM.Controllers
{
    [Authorize]
    public class FinansController : Controller
    {
        private readonly CrmDbContext _context;
        public FinansController(CrmDbContext context) => _context = context;

        public async Task<IActionResult> KasaHareketi()
        {
            ViewBag.Musteriler = await _context.Musterilers
                .AsNoTracking()
                .Where(x => !x.SilindiMi)
                .OrderBy(x => x.AdSoyad)
                .ToListAsync();
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetFinansOzetGrafik()
        {
            var islemler = await _context.KasaHareketleris.AsNoTracking()
                .GroupBy(x => x.IslemTipi ?? "Genel Tahsilat")
                .Select(g => new { etiket = g.Key, toplam = g.Sum(s => s.Tutar ?? 0) })
                .ToListAsync();

            var yontemler = await _context.KasaHareketleris.AsNoTracking()
                .GroupBy(x => x.OdemeYontemi ?? "Belirtilmemiş")
                .Select(g => new { etiket = g.Key, toplam = g.Sum(s => s.Tutar ?? 0) })
                .ToListAsync();

            var cihazlar = await (from s in _context.ServisKayitlaris
                                  join c in _context.MusteriCihazlaris on s.MusteriCihazId equals c.Id
                                  group c by c.Marka into g
                                  select new { etiket = g.Key, toplam = (decimal)g.Count() })
                                  .ToListAsync();

            return Json(new { islemler, yontemler, cihazlar });
        }

        public async Task<IActionResult> ExcelIndir()
        {
            var veriler = await (from k in _context.KasaHareketleris
                                 join m in _context.Musterilers on k.MusteriId equals m.Id into mj
                                 from m in mj.DefaultIfEmpty()
                                 orderby k.Tarih descending
                                 select new { k.Tarih, MusteriAd = m != null ? m.AdSoyad : "Genel Cari", k.IslemTipi, k.OdemeYontemi, k.Tutar })
                                 .AsNoTracking().ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Kasa Hareketleri");
                var headers = new string[] { "Tarih", "Müşteri / Cari", "İşlem Tipi", "Yöntem", "Tutar" };
                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4e73df");
                    cell.Style.Font.FontColor = XLColor.White;
                }
                int row = 2;
                foreach (var item in veriler)
                {
                    worksheet.Cell(row, 1).Value = item.Tarih?.ToString("dd.MM.yyyy HH:mm") ?? "-";
                    worksheet.Cell(row, 2).Value = item.MusteriAd;
                    worksheet.Cell(row, 3).Value = item.IslemTipi ?? "Tahsilat";
                    worksheet.Cell(row, 4).Value = item.OdemeYontemi;
                    worksheet.Cell(row, 5).Value = item.Tutar ?? 0;
                    worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00\" ₺\"";
                    row++;
                }
                worksheet.Columns().AdjustToContents();
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Kasa_Raporu_{DateTime.Now:yyyyMMdd}.xlsx");
                }
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetKasaListesi()
        {
            var liste = await (from k in _context.KasaHareketleris
                               join m in _context.Musterilers on k.MusteriId equals m.Id into mj
                               from m in mj.DefaultIfEmpty()
                               orderby k.Tarih descending
                               select new { k.Id, tarih = k.Tarih.HasValue ? k.Tarih.Value.ToString("dd.MM.yyyy HH:mm") : "-", musteriAd = m != null ? m.AdSoyad : "Genel Cari", k.OdemeYontemi, k.IslemTipi, k.Tutar })
                               .AsNoTracking().ToListAsync();
            return Json(liste);
        }

        [HttpGet]
        public async Task<JsonResult> GetKasaDetay(int id)
        {
            var k = await _context.KasaHareketleris.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (k == null) return Json(null);
            var m = await _context.Musterilers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == k.MusteriId);
            return Json(new { id = k.Id, tarih = k.Tarih.HasValue ? k.Tarih.Value.ToString("dd.MM.yyyy HH:mm") : "-", musteriAd = m?.AdSoyad ?? "Genel Cari", tutar = k.Tutar, odemeYontemi = k.OdemeYontemi, islemTipi = k.IslemTipi });
        }

        [HttpPost]
        public async Task<JsonResult> KasaSil(int id)
        {
            var k = await _context.KasaHareketleris.FindAsync(id);
            if (k != null) { _context.KasaHareketleris.Remove(k); await _context.SaveChangesAsync(); return Json(new { success = true }); }
            return Json(new { success = false });
        }
        [HttpPost]
        public async Task<JsonResult> OdemeAl(KasaHareketleri model)
        {
            try
            {
                model.Tarih = DateTime.Now;
                // Eğer seçilen kategori "Gider" ise tutarı negatif olarak kaydet
                if (model.IslemTipi == "Gider")
                {
                    model.Tutar = -Math.Abs(model.Tutar ?? 0);
                }
                else
                {
                    model.Tutar = Math.Abs(model.Tutar ?? 0);
                }
                _context.KasaHareketleris.Add(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false }); }
        }

        [HttpPost]
        public async Task<JsonResult> HizliOdemeAl(int servisId, string odemeYontemi)
        {
            try
            {
                if (servisId <= 0) return Json(new { success = false, message = "Geçersiz ID!" });

                // Modelindeki tablo adı ServisKayitlari, anahtar Id
                var servis = await _context.ServisKayitlaris.FindAsync(servisId);

                if (servis == null) return Json(new { success = false, message = "Kayıt bulunamadı." });

                servis.OdemeDurumu = "Ödendi";
                servis.KapanisTarihi = DateTime.Now; // Ödeme tarihini güncelle

                var kasa = new KasaHareketleri
                {
                    MusteriId = servis.MusteriId,
                    Tutar = servis.Ucret ?? 0,
                    Tarih = DateTime.Now,
                    IslemTipi = "Servis Tahsilatı",
                    OdemeYontemi = odemeYontemi ?? "Nakit"
                };

                _context.KasaHareketleris.Add(kasa);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }
    }
}