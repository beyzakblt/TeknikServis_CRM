using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TeknikServis_CRM.Models;
using Microsoft.EntityFrameworkCore;

namespace TeknikServis_CRM.Controllers
{
    [Authorize]
    public class MusteriController : Controller
    {
        private readonly CrmDbContext _context;
        public MusteriController(CrmDbContext context) => _context = context;

        // Ana sayfa yüklemesi
        public IActionResult Index() => View();

        // Müşteri Listesini getiren AJAX metodu
        [HttpGet]
        public async Task<IActionResult> GetMusteriListesi()
        {
            var musteriler = await _context.Musterilers
                .OrderByDescending(m => m.KayitTarihi)
                .ToListAsync();
            return Json(musteriler);
        }

        // Müşteri Ekleme AJAX metodu
        [HttpPost]
        public async Task<IActionResult> Ekle(Musteriler model)
        {
            try
            {
                model.KayitTarihi = DateTime.Now;
                // Modelinizdeki zorunlu alanları (Durum vb.) burada set edebilirsiniz
                // model.Durum = "Aktif"; 

                _context.Musterilers.Add(model);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Müşteri başarıyla kaydedildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Sil(int id)
        {
            var musteri = await _context.Musterilers.FindAsync(id);
            if (musteri == null) return Json(new { success = false, message = "Müşteri bulunamadı!" });

            _context.Musterilers.Remove(musteri);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Müşteri başarıyla silindi." });
        }

        [HttpGet]
        public async Task<IActionResult> GetMusteri(int id)
        {
            var musteri = await _context.Musterilers.FindAsync(id);
            return Json(musteri);
        }

        [HttpPost]
        public async Task<IActionResult> Guncelle(Musteriler model)
        {
            var existing = await _context.Musterilers.FindAsync(model.Id);
            if (existing == null) return Json(new { success = false, message = "Kayıt bulunamadı!" });

            existing.Ad = model.Ad;
            existing.Eposta = model.Eposta;
            existing.Telefon = model.Telefon;
            existing.Adres = model.Adres;

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Müşteri güncellendi." });
        }
    }
}