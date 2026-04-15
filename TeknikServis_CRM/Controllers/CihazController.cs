using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TeknikServis_CRM.Models;
using TeknikServis_CRM.Helpers; // YetkiServis'e erişmek için
using Microsoft.AspNetCore.Authorization;

namespace TeknikServis_CRM.Controllers
{
    [Authorize] // Sadece giriş yapmış kullanıcılar erişebilir
    public class CihazController : Controller
    {
        private readonly CrmDbContext _context;
        public CihazController(CrmDbContext context) => _context = context;

        // --- 1. LİSTELEME SAYFASI ---
        [HttpGet]
        public IActionResult Index()
        {
            var musteriler = _context.Musterilers
                .Select(m => new
                {
                    id = m.Id,
                    adSoyad = m.Ad + " " + m.Soyad
                }).ToList();

            ViewBag.Musteriler = new SelectList(musteriler, "id", "adSoyad");
            return View();
        }

        // --- 2. AJAX İLE LİSTE GETİR ---
        [HttpGet]
        public async Task<IActionResult> GetCihazListesi()
        {
            var cihazlar = await _context.Cihazlars
                .Join(_context.Musterilers,
                    c => c.MusteriId,
                    m => m.Id,
                    (c, m) => new
                    {
                        id = c.Id,
                        marka = c.Marka ?? "",
                        model = c.Model ?? "",
                        seriNo = c.SeriNo ?? "",
                        cihazNotu = c.CihazNotu ?? "",
                        musteriAd = m.Ad + " " + m.Soyad
                    })
                .OrderByDescending(x => x.id)
                .ToListAsync();

            return Json(cihazlar);
        }

        // --- 3. TEK CİHAZ GETİR (DÜZENLEME İÇİN) ---
        [HttpGet]
        public async Task<IActionResult> GetCihaz(int id)
        {
            var c = await _context.Cihazlars.FindAsync(id);
            if (c == null) return Json(null);

            return Json(new
            {
                id = c.Id,
                marka = c.Marka,
                model = c.Model,
                seriNo = c.SeriNo,
                cihazNotu = c.CihazNotu,
                musteriId = c.MusteriId
            });
        }

        // --- 4. CİHAZ EKLE ---
        [HttpPost]
        public async Task<IActionResult> Ekle(Cihazlar model)
        {
            // --- YETKİ KONTROLÜ ---
            int userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (!YetkiServis.YetkiKontrol(userId, "CIHAZ_YONETIM", "E", _context))
            {
                return Json(new { success = false, message = "Cihaz ekleme yetkiniz bulunmuyor!" });
            }

            // Form verilerini kontrol et
            if (model.MusteriId == 0 && Request.Form.ContainsKey("MusteriId"))
            {
                model.MusteriId = int.Parse(Request.Form["MusteriId"]!);
                model.Marka = Request.Form["Marka"];
                model.Model = Request.Form["Model"];
                model.SeriNo = Request.Form["SeriNo"];
                model.CihazNotu = Request.Form["CihazNotu"];
            }

            if (model.MusteriId > 0)
            {
                _context.Cihazlars.Add(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Zorunlu alanları doldurunuz!" });
        }

        // --- 5. CİHAZ GÜNCELLE ---
        [HttpPost]
        public async Task<IActionResult> Guncelle(Cihazlar model)
        {
            // --- YETKİ KONTROLÜ ---
            int userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (!YetkiServis.YetkiKontrol(userId, "CIHAZ_YONETIM", "D", _context))
            {
                return Json(new { success = false, message = "Cihaz güncelleme yetkiniz bulunmuyor!" });
            }

            var c = await _context.Cihazlars.FindAsync(model.Id);
            if (c == null) return Json(new { success = false, message = "Cihaz bulunamadı!" });

            c.Marka = model.Marka;
            c.Model = model.Model;
            c.SeriNo = model.SeriNo;
            c.CihazNotu = model.CihazNotu;
            c.MusteriId = model.MusteriId;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // --- 6. CİHAZ SİL ---
        [HttpPost]
        public async Task<IActionResult> Sil(int id)
        {
            // --- YETKİ KONTROLÜ ---
            int userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            if (!YetkiServis.YetkiKontrol(userId, "CIHAZ_YONETIM", "S", _context))
            {
                return Json(new { success = false, message = "Bu cihazı silme yetkiniz bulunmuyor!" });
            }

            var c = await _context.Cihazlars.FindAsync(id);
            if (c != null)
            {
                _context.Cihazlars.Remove(c);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Cihaz bulunamadı!" });
        }
    }
}