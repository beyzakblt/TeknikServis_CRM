#nullable disable
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeknikServis_CRM.Models;
using TeknikServis_CRM.Services;

namespace TeknikServis_CRM.Controllers
{
    public class IletisimController : Controller
    {
        private readonly CrmDbContext _context;
        private readonly EmailService _emailService;

        public IletisimController(CrmDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // ============================================================
        // DASHBOARD: ÖDEME HATIRLATMA MAİLİ GÖNDERİMİ
        // ============================================================
        [HttpPost]
        public async Task<JsonResult> OdemeHatirlat(int musteriId, string tutar)
        {
            try
            {
                var musteri = await _context.Musterilers.FindAsync(musteriId);

                if (musteri == null)
                    return Json(new { success = false, message = "Müşteri kaydı bulunamadı." });

                // Mail boş mu veya geçersiz mi kontrolü
                if (string.IsNullOrEmpty(musteri.Eposta) || !musteri.Eposta.Contains("@"))
                    return Json(new { success = false, message = "Geçersiz e-posta adresi! Lütfen müşteri kartını güncelleyin." });

                string konu = "Ödeme Hatırlatması | Teknik Servis";
                string mailIcerigi = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                        <h2 style='color: #2d3748;'>Sayın {musteri.AdSoyad},</h2>
                        <p>Sistemimizde kayıtlı olan <strong>{tutar}</strong> tutarındaki ödemenizin vadesi yaklaşmaktadır.</p>
                        <p>Ödemenizi nakit, kredi kartı veya havale yoluyla gerçekleştirebilirsiniz.</p>
                        <br>
                        <p style='color: #718096; font-size: 12px;'>Bu mail teknik sistem tarafından otomatik olarak gönderilmiştir.</p>
                        <hr>
                        <p><strong>Teknik CRM Destek Ekibi</strong></p>
                    </div>";

                // Email gönderme işlemi
                bool sonuc = await _emailService.SendEmailAsync(musteri.Eposta, konu, mailIcerigi);

                if (sonuc)
                {
                    _context.MusteriAktiviteLoglaris.Add(new MusteriAktiviteLoglari
                    {
                        MusteriId = musteriId,
                        IslemTipi = "Ödeme Hatırlatması",
                        Aciklama = $"Tutar: {tutar} için mail gönderildi.",
                        Tarih = DateTime.Now
                    });
                    await _context.SaveChangesAsync();

                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "SMTP Sunucusu maili reddetti. Bağlantı ayarlarınızı kontrol edin." });
            }
            catch (Exception ex)
            {
                // Hatanın detayını döndürerek JS tarafında görmeni sağlıyoruz
                return Json(new { success = false, message = "Sistem Hatası: " + ex.Message });
            }
        }

        // ============================================================
        // TOPLU VEYA TEKLİ MAİL GÖNDERİMİ
        // ============================================================
        [HttpPost]
        public async Task<JsonResult> MesajGonder(List<int> HedefIdler, string Konu, string Mesaj)
        {
            try
            {
                if (HedefIdler == null || !HedefIdler.Any())
                    return Json(new { success = false, message = "Lütfen alıcı seçiniz." });

                int basariliSayisi = 0;
                int hataliSayisi = 0;

                foreach (var id in HedefIdler)
                {
                    var musteri = await _context.Musterilers.FindAsync(id);

                    if (musteri != null && !string.IsNullOrEmpty(musteri.Eposta) && musteri.Eposta.Contains("@"))
                    {
                        string mailIcerigi = $"Sayın {musteri.AdSoyad},<br/><br/>{Mesaj}<br/><br/>Saygılarımızla,<br/>Teknik CRM Ekibi";
                        bool sonuc = await _emailService.SendEmailAsync(musteri.Eposta, Konu, mailIcerigi);

                        if (sonuc)
                        {
                            basariliSayisi++;
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
                return Json(new { success = true, message = $"{basariliSayisi} başarılı, {hataliSayisi} hatalı gönderim." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // ============================================================
        // TOPLU İŞ EMRİ ATAMA
        // ============================================================
        [HttpPost]
        public async Task<JsonResult> TopluIsEmriAtay(List<int> MusteriIdler, int TeknisyenId, string Aciklama)
        {
            try
            {
                foreach (var mId in MusteriIdler)
                {
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
                return Json(new { success = true, message = "İş emirleri başarıyla atandı." });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
    }
}