#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeknikServis_CRM.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TeknikServis_CRM.Controllers
{
    [Authorize]
    public class CihazController : Controller
    {
        private readonly CrmDbContext _context;
        public CihazController(CrmDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var musteriler = await _context.Musterilers
                .Where(m => m.SilindiMi == false)
                .Select(m => new { id = m.Id, ad = m.AdSoyad })
                .ToListAsync();

            var tipler = await _context.CihazTipleris.ToListAsync();

            ViewBag.Musteriler = new SelectList(musteriler, "id", "ad");
            ViewBag.CihazTipleri = new SelectList(tipler, "Id", "TipAdi");

            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetCihazListesi(int? musteriId, int? cihazTipId, string marka)
        {
            // HATA BURADAYDI: Cihazlars yerine senin modeline uygun olan MusteriCihazlaris yapıldı.
            var query = _context.MusteriCihazlaris
                .Include(c => c.Musteri)
                .Include(c => c.CihazTip)
                .AsQueryable();

            // Filtre 1: Müşteriye göre
            if (musteriId.HasValue)
                query = query.Where(c => c.MusteriId == musteriId.Value);

            // Filtre 2: Cihaz Tipine göre
            if (cihazTipId.HasValue)
                query = query.Where(c => c.CihazTipId == cihazTipId.Value);

            // Filtre 3: Markaya göre (İçerisinde geçiyorsa)
            if (!string.IsNullOrEmpty(marka))
                query = query.Where(c => c.Marka.Contains(marka));

            var data = await query.Select(c => new
            {
                c.Id,
                MusteriAd = c.Musteri != null ? c.Musteri.AdSoyad : "Bilinmiyor",
                CihazTipi = c.CihazTip != null ? c.CihazTip.TipAdi : "Belirtilmemiş",
                c.Marka,
                c.Model,
                c.SeriNo
            }).ToListAsync();

            return Json(data);
        }

        [HttpPost]
        public async Task<JsonResult> Kaydet(MusteriCihazlari cihazInput)
        {
            try
            {
                if (cihazInput.MusteriId == 0) return Json(new { success = false, message = "Lütfen bir müşteri seçin." });

                if (cihazInput.Id == 0)
                {
                    cihazInput.KurulumTarihi = DateTime.Now;
                    cihazInput.AktifMi = true;
                    _context.MusteriCihazlaris.Add(cihazInput);
                }
                else
                {
                    var ent = await _context.MusteriCihazlaris.FindAsync(cihazInput.Id);
                    if (ent == null) return Json(new { success = false, message = "Kayıt bulunamadı!" });

                    ent.MusteriId = cihazInput.MusteriId;
                    ent.CihazTipId = cihazInput.CihazTipId;
                    ent.Marka = cihazInput.Marka;
                    ent.Model = cihazInput.Model;
                    ent.SeriNo = cihazInput.SeriNo;
                    ent.AktifMi = true;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cihaz başarıyla kaydedildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + (ex.InnerException?.Message ?? ex.Message) });
            }
        }

        [HttpPost]
        public async Task<JsonResult> Sil(int id)
        {
            var c = await _context.MusteriCihazlaris.FindAsync(id);
            if (c != null)
            {
                _context.MusteriCihazlaris.Remove(c);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpGet]
        public async Task<JsonResult> GetCihaz(int id)
        {
            var data = await _context.MusteriCihazlaris.FindAsync(id);
            return Json(data);
        }
    }
}