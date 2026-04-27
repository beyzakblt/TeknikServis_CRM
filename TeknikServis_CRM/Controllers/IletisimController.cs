#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeknikServis_CRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TeknikServis_CRM.Controllers
{
    public class IletisimController : Controller
    {
        private readonly CrmDbContext _context;
        public IletisimController(CrmDbContext context) => _context = context;

        // ============================================================
        // TOPLU VEYA TEKLİ MAİL / MESAJ GÖNDERİMİ (SİSTEM LOGU)
        // ============================================================
        [HttpPost]
        public async Task<JsonResult> MesajGonder(List<int> HedefIdler, string Konu, string Mesaj)
        {
            try
            {
                if (HedefIdler == null || !HedefIdler.Any())
                    return Json(new { success = false, message = "Lütfen alıcı seçiniz." });

                var emailService = new TeknikServis_CRM.Services.EmailService();
                int basariliSayisi = 0;
                int hataliSayisi = 0;

                foreach (var id in HedefIdler)
                {
                    var musteri = await _context.Musterilers.FindAsync(id);

                    // Müşteri varsa ve geçerli bir e-posta adresi tanımlıysa
                    if (musteri != null && !string.IsNullOrEmpty(musteri.Eposta))
                    {
                        // Mail gövdesini kişiselleştirebiliriz (Opsiyonel)
                        string mailIcerigi = $"Sayın {musteri.AdSoyad},<br/><br/>{Mesaj}<br/><br/>Saygılarımızla,<br/>Teknik CRM Ekibi";

                        bool sonuc = await emailService.SendEmailAsync(musteri.Eposta, Konu, mailIcerigi);

                        if (sonuc)
                        {
                            basariliSayisi++;
                            // Veritabanına iletişim logu atalım
                            _context.MusteriAktiviteLoglaris.Add(new MusteriAktiviteLoglari
                            {
                                MusteriId = id,
                                IslemTipi = "E-Posta Gönderildi",
                                Aciklama = $"Konu: {Konu}",
                                Tarih = DateTime.Now
                            });
                        }
                        else { hataliSayisi++; }
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message = $"{basariliSayisi} e-posta başarıyla gönderildi. {(hataliSayisi > 0 ? hataliSayisi + " hata oluştu." : "")}"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Sistem hatası: " + ex.Message });
            }
        }

        // ============================================================
        // TOPLU İŞ EMRİ ATAMA (TEKNİSYENLERE)
        // ============================================================
        [HttpPost]
        public async Task<JsonResult> TopluIsEmriAtay(List<int> MusteriIdler, int TeknisyenId, string Aciklama)
        {
            try
            {
                foreach (var mId in MusteriIdler)
                {
                    // Müşterinin aktif bir cihazını bul (varsayılan)
                    var cihaz = await _context.MusteriCihazlaris.FirstOrDefaultAsync(x => x.MusteriId == mId && x.AktifMi == true);

                    _context.IsEmirleris.Add(new IsEmirleri
                    {
                        MusteriId = mId,
                        CihazId = cihaz?.Id,
                        TeknisyenId = TeknisyenId,
                        ArizaAciklamasi = Aciklama,
                        Durum = "Yeni Atandı",
                        OlusturmaTarihi = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "İş emirleri teknisyene atandı." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
    }
}