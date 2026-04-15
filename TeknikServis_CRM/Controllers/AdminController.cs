using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis_CRM.Models;
using Microsoft.AspNetCore.Authorization;

namespace TeknikServis_CRM.Controllers
{
    [Authorize(Roles = "Admin")] // Tüm sayfa sadece Admin yetkisi olanlara açık
    public class AdminController : Controller
    {
        private readonly CrmDbContext _context;

        public AdminController(CrmDbContext context)
        {
            _context = context;
        }

        // --- 1. ROLLERİ LİSTELE ---
        [HttpGet]
        public async Task<IActionResult> Roller()
        {
            // Popup Modal içindeki yetki satırlarını (Cihazlar, Kullanıcılar vb.) oluşturmak için 
            // veritabanındaki tüm yetki tanımlarını ViewBag ile sayfaya gönderiyoruz.
            ViewBag.YetkiListesi = await _context.Yetkilers.Where(x => x.Durum == "Aktif").ToListAsync();

            var roller = await _context.Rollers.ToListAsync();
            return View(roller);
        }

        // --- 2. ROL DETAYLARINI GETİR (AJAX - Modal Doldurma) ---
        [HttpGet]
        public async Task<IActionResult> RolKaydet(int id)
        {
            var rol = await _context.Rollers.FindAsync(id);
            if (rol == null) return Json(null);

            // Seçilen role ait veritabanında kayıtlı olan yetki matrisini çekiyoruz
            var yetkiler = await _context.RolYetkileris
                .Where(x => x.RolId == id)
                .ToListAsync();

            // JavaScript tarafındaki editRol fonksiyonunun beklediği JSON formatı
            return Json(new { rol = rol, yetkiler = yetkiler });
        }

        // --- 3. ROL VE YETKİ MATRİSİNİ KAYDET (POST) ---
        [HttpPost]
        public async Task<IActionResult> RolKaydet(Roller model, List<RolYetkileri>? YetkiMatrisi)
        {
            try
            {
                if (model.Id == 0) // --- YENİ KAYIT ---
                {
                    _context.Rollers.Add(model);
                }
                else // --- GÜNCELLEME ---
                {
                    var mevcutRol = await _context.Rollers.FindAsync(model.Id);
                    if (mevcutRol == null)
                        return Json(new { success = false, message = "Güncellenecek rol bulunamadı!" });

                    mevcutRol.RolAdi = model.RolAdi;
                    mevcutRol.RolKodu = model.RolKodu;
                    mevcutRol.Durum = model.Durum;

                    _context.Rollers.Update(mevcutRol);
                }

                // Önce rol bilgilerini kaydediyoruz (Id'nin oluşması için)
                await _context.SaveChangesAsync();

                // --- YETKİ MATRİSİ YÖNETİMİ ---
                // Mevcut role ait eski tüm yetkileri siliyoruz (FK olmadığı için güvenli)
                var eskiYetkiler = _context.RolYetkileris.Where(x => x.RolId == model.Id);
                _context.RolYetkileris.RemoveRange(eskiYetkiler);

                // Eğer formdan yeni matris verisi geldiyse döngüyle ekliyoruz
                if (YetkiMatrisi != null && YetkiMatrisi.Any())
                {
                    foreach (var yetki in YetkiMatrisi)
                    {
                        // Sadece en az bir kutucuğu (G,E,D,S) işaretli olan satırları kaydedelim
                        if (yetki.GoruntulemeVarMi || yetki.EklemeVarMi || yetki.GuncellemeVarMi || yetki.SilmeVarMi)
                        {
                            yetki.RolId = model.Id;
                            _context.RolYetkileris.Add(yetki);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Rol ve yetki matrisi başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Hata oluştu: " + ex.Message });
            }
        }

        // --- 4. ROLÜ PASİFE ÇEK (SİL) ---
        [HttpPost]
        public async Task<IActionResult> RolSil(int id)
        {
            var rol = await _context.Rollers.FindAsync(id);
            if (rol != null)
            {
                // Kurumsal projelerde silmek yerine Durum='Pasif' yapmak verinin geçmişini korur.
                rol.Durum = "Pasif";
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Rol başarıyla pasif duruma getirildi." });
            }
            return Json(new { success = false, message = "Rol bulunamadı." });
        }
    }
}