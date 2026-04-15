using TeknikServis_CRM.Models;
using Microsoft.EntityFrameworkCore;

namespace TeknikServis_CRM.Helpers
{
    public static class YetkiServis
    {
        /// <summary>
        /// Kullanıcının belirli bir modülde yetkisi olup olmadığını kontrol eder.
        /// </summary>
        /// <param name="kullaniciId">Giriş yapan kullanıcı ID</param>
        /// <param name="yetkiKodu">SQL'deki YetkiKodu (Örn: CIHAZ_YONETIM)</param>
        /// <param name="islem">G: Görüntüle, E: Ekle, D: Düzenle, S: Sil</param>
        public static bool YetkiKontrol(int kullaniciId, string yetkiKodu, string islem, CrmDbContext _context)
        {
            // 1. Kullanıcının sahip olduğu rollerin listesini al
            var kullaniciRolleri = _context.KullaniciRolleris
                .Where(x => x.KullaniciId == kullaniciId)
                .Select(x => x.RolId)
                .ToList();

            if (!kullaniciRolleri.Any()) return false;

            // 2. Bu rollerden herhangi biri, istenen yetki koduna ve işlem tipine sahip mi?
            return _context.RolYetkileris.Any(ry =>
                kullaniciRolleri.Contains(ry.RolId) &&
                _context.Yetkilers.Any(y => y.Id == ry.YetkiId && y.YetkiKodu == yetkiKodu) &&
                (
                    (islem == "G" && ry.GoruntulemeVarMi) ||
                    (islem == "E" && ry.EklemeVarMi) ||
                    (islem == "D" && ry.GuncellemeVarMi) ||
                    (islem == "S" && ry.SilmeVarMi)
                )
            );
        }
    }
}