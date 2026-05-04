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
    public class MusteriController : Controller
    {
        private readonly CrmDbContext _context;
        public MusteriController(CrmDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            ViewBag.MusteriTipleri = await _context.MusteriTipleris.AsNoTracking().ToListAsync();
            ViewBag.MusteriDurumlari = await _context.MusteriDurumlaris.AsNoTracking().ToListAsync();
            ViewBag.Teknisyenler = await (from k in _context.Kullanicilars
                                          join kr in _context.KullaniciRolleris on k.Id equals kr.KullaniciId
                                          join r in _context.Rollers on kr.RolId equals r.Id
                                          where r.RolKodu == "TEK" || r.RolAdi.Contains("Teknisyen")
                                          select new { k.Id, AdSoyad = k.Ad + " " + k.Soyad }).Distinct().ToListAsync();
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetMusteriListesi()
        {
            var liste = await _context.Musterilers
                .AsNoTracking()
                .Select(m => new {
                    m.Id,
                    m.AdSoyad,
                    m.FirmaAdi,
                    m.Telefon,
                    m.Eposta,
                    DurumAdi = m.MusteriDurum != null ? m.MusteriDurum.DurumAdi : "Tanımsız",
                    RenkKodu = m.MusteriDurum != null ? m.MusteriDurum.RenkKodu : "#6c757d",
                    TipAdi = m.MusteriTip != null ? m.MusteriTip.TipAdi : "Genel",
                    ToplamTahsilat = _context.KasaHareketleris.Where(k => k.MusteriId == m.Id).Sum(k => (decimal?)k.Tutar) ?? 0
                }).ToListAsync();
            return Json(liste);
        }

        [HttpGet]
        public async Task<JsonResult> GetFullDetay(int id)
        {
            var m = await _context.Musterilers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (m == null) return Json(new { success = false });

            // Toplam Borç - Toplam Tahsilat = Kalan Bakiye
            // Not: Senin sistemindeki ServisÜcreti veya Borç tablosuna göre burayı revize edebilirsin.
            var toplamBorc = await _context.ServisKayitlaris.Where(s => s.MusteriId == id).SumAsync(s => (decimal?)s.Ucret) ?? 0;
            var toplamTahsilat = await _context.KasaHareketleris.Where(k => k.MusteriId == id).SumAsync(k => (decimal?)k.Tutar) ?? 0;
            var kalanBakiye = toplamBorc - toplamTahsilat;

            return Json(new
            {
                success = true,
                musteri = new { m.Id, m.AdSoyad, m.FirmaAdi, m.Telefon, m.Eposta },
                kalanBakiye = kalanBakiye, // Bu yeni eklendi
                cihazlar = await _context.MusteriCihazlaris.Where(c => c.MusteriId == id).Select(c => new { c.Id, c.Marka, c.Model }).ToListAsync(),
                notlar = await _context.MusteriNotlaris.Where(n => n.MusteriId == id).OrderByDescending(x => x.Tarih).Select(n => new { n.NotIcerigi, Tarih = n.Tarih.Value.ToString("dd.MM.yyyy HH:mm") }).ToListAsync(),
                finans = await _context.KasaHareketleris.Where(k => k.MusteriId == id).OrderByDescending(x => x.Tarih).Select(f => new { f.Tarih, f.Tutar, f.IslemTipi }).ToListAsync(),
                toplamCiro = toplamTahsilat
            });
        }

        [HttpPost]
        public async Task<JsonResult> Kaydet(Musteriler model, string AcikAdres)
        {
            try
            {
                if (model.Id == 0)
                {
                    model.OlusturmaTarihi = DateTime.Now;
                    model.AdSoyad = (model.Ad + " " + model.Soyad).Trim();
                    _context.Musterilers.Add(model);
                }
                else
                {
                    var ent = await _context.Musterilers.FindAsync(model.Id);
                    if (ent == null) return Json(new { success = false, message = "Kayıt bulunamadı." });
                    ent.Ad = model.Ad;
                    ent.Soyad = model.Soyad;
                    ent.AdSoyad = (model.Ad + " " + model.Soyad).Trim();
                    ent.FirmaAdi = model.FirmaAdi;
                    ent.Telefon = model.Telefon;
                    ent.Eposta = model.Eposta;
                    ent.MusteriTipId = model.MusteriTipId;
                    ent.MusteriDurumId = model.MusteriDurumId;
                }
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(AcikAdres))
                {
                    try
                    {
                        var adr = await _context.MusteriAdresleris.FirstOrDefaultAsync(a => a.MusteriId == model.Id);
                        if (adr == null)
                            _context.MusteriAdresleris.Add(new MusteriAdresleri { MusteriId = model.Id, AcikAdres = AcikAdres, OlusturmaTarihi = DateTime.Now });
                        else
                            adr.AcikAdres = AcikAdres;
                        await _context.SaveChangesAsync();
                    }
                    catch { /* Adres hatası kaydı tamamen durdurmasın */ }
                }
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpGet]
        public async Task<JsonResult> GetMusteri(int id)
        {
            var m = await _context.Musterilers.FindAsync(id);
            var a = await _context.MusteriAdresleris.FirstOrDefaultAsync(x => x.MusteriId == id);
            return Json(new { musteri = m, adres = a });
        }

        [HttpPost]
        public async Task<JsonResult> Sil(int id)
        {
            var m = await _context.Musterilers.FindAsync(id);
            if (m != null) { _context.Musterilers.Remove(m); await _context.SaveChangesAsync(); }
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<JsonResult> NotEkle(int MusteriId, string NotIcerigi)
        {
            _context.MusteriNotlaris.Add(new MusteriNotlari { MusteriId = MusteriId, NotIcerigi = NotIcerigi, Tarih = DateTime.Now });
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> HizliTahsilat(int musteriId, decimal tutar, string yontem)
        {
            try
            {
                var kasa = new KasaHareketleri
                {
                    MusteriId = musteriId,
                    Tutar = tutar,
                    Tarih = DateTime.Now,
                    IslemTipi = "Tahsilat",
                    OdemeYontemi = yontem ?? "Nakit"
                };
                _context.KasaHareketleris.Add(kasa);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        // Servis Kaydı Başlatma Metodu (ServisKayitController'da değilse buraya ekle)
        [HttpPost]
        public async Task<JsonResult> HizliServisKaydiAc(int MusteriId, int CihazId, string SikayetAciklamasi)
        {
            try
            {
                var yeniServis = new ServisKayitlari
                {
                    MusteriId = MusteriId,
                    MusteriCihazId = CihazId,
                    KapanisTarihi = DateTime.Now,
                    ServisDurumId = 1, // 'Beklemede' veya 'Yeni Kayıt' ID'si
                    ServisNo = "SRV-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper()
                };
                _context.ServisKayitlaris.Add(yeniServis);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
    }
}