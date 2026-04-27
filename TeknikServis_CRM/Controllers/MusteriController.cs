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
            return View();
        }

        // ============================================================
        // LİSTE: DataTable için verileri toplu çeker
        // ============================================================
        [HttpGet]
        public async Task<JsonResult> GetMusteriListesi(bool pasifler = false)
        {
            var liste = await _context.Musterilers
                .AsNoTracking()
                .Where(m => m.SilindiMi == pasifler)
                .Select(m => new
                {
                    m.Id,
                    m.AdSoyad,
                    m.FirmaAdi,
                    m.Telefon,
                    m.Eposta,
                    DurumAdi = m.MusteriDurum != null ? m.MusteriDurum.DurumAdi : "Tanımsız",
                    RenkKodu = m.MusteriDurum != null ? m.MusteriDurum.RenkKodu : "#6c757d",
                    TipAdi = m.MusteriTip != null ? m.MusteriTip.TipAdi : "Genel",
                    m.SonServisTarihi,
                    ToplamTahsilat = _context.KasaHareketleris
                                        .Where(k => k.MusteriId == m.Id)
                                        .Sum(k => (decimal?)k.Tutar) ?? 0
                })
                .ToListAsync();

            return Json(liste);
        }

        // ============================================================
        // FULL DETAY (360 DERECE KOMUTA MERKEZİ)
        // ============================================================
        [HttpGet]
        public async Task<JsonResult> GetFullDetay(int id)
        {
            try
            {
                var m = await _context.Musterilers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
                if (m == null) return Json(new { success = false, message = "Müşteri bulunamadı." });

                var cihazlar = await _context.MusteriCihazlaris.AsNoTracking().Where(c => c.MusteriId == id).ToListAsync();
                var notlar = await _context.MusteriNotlaris.AsNoTracking().Where(n => n.MusteriId == id).OrderByDescending(x => x.Tarih).ToListAsync();
                var adresler = await _context.MusteriAdresleris.AsNoTracking().Where(a => a.MusteriId == id).ToListAsync();
                var loglar = await _context.MusteriAktiviteLoglaris.AsNoTracking().Where(l => l.MusteriId == id).OrderByDescending(x => x.Tarih).ToListAsync();
                var finans = await _context.KasaHareketleris.AsNoTracking().Where(k => k.MusteriId == id).OrderByDescending(x => x.Tarih).ToListAsync();

                return Json(new
                {
                    success = true,
                    musteri = new
                    {
                        m.Id,
                        m.AdSoyad,
                        m.FirmaAdi,
                        m.YetkiliKisi,
                        m.Telefon,
                        m.Eposta,
                        m.VergiDairesi,
                        m.VergiNo,
                        m.Aciklama,
                        KayitTarihi = m.OlusturmaTarihi.HasValue ? m.OlusturmaTarihi.Value.ToString("dd.MM.yyyy") : "-"
                    },
                    // BURAYI GÜNCELLEDİK: GorselYolu eklendi
                    cihazlar = cihazlar.Select(c => new { c.Id, c.Marka, c.Model, SeriNo = c.SeriNo ?? "-", c.GorselYolu }).ToList(),
                    notlar = notlar.Select(n => new { n.NotIcerigi, Tarih = n.Tarih.HasValue ? n.Tarih.Value.ToString("dd.MM.yyyy HH:mm") : "-" }).ToList(),
                    adresler = adresler.Select(a => new { AdresTipi = a.AdresTipi ?? "Merkez", a.Il, a.AcikAdres }).ToList(),
                    loglar = loglar.Select(l => new { l.IslemTipi, l.Aciklama, Tarih = l.Tarih.HasValue ? l.Tarih.Value.ToString("dd.MM.yyyy HH:mm") : "-" }).ToList(),
                    finans = finans.Select(f => new { f.Id, Tarih = f.Tarih.HasValue ? f.Tarih.Value.ToString("dd.MM.yyyy") : "-", Tutar = f.Tutar ?? 0, f.IslemTipi }).ToList(),
                    toplamCiro = finans.Sum(f => f.Tutar) ?? 0
                });
            }
            catch (Exception ex) { return Json(new { success = false, message = "Hata: " + ex.Message }); }
        }

        // ============================================================
        // KAYDET (TÜM BİLGİLER + İL VE ADRES DAHİL)
        // ============================================================
        [HttpPost]
        public async Task<JsonResult> Kaydet(Musteriler model, string AdresTipi, string Il, string AcikAdres)
        {
            try
            {
                ModelState.Clear();

                // EF Core'un boş koleksiyonları kaydetmeye çalışmasını ve patlamasını engeller
                model.MusteriAdresleri = null;
                model.MusteriNotlari = null;
                model.MusteriBelgeleri = null;
                model.MusteriAktiviteLoglari = null;
                model.MusteriEtiketAtamalari = null;
                model.MusteriCihazlari = null;

                int musteriId = model.Id;

                if (model.Id == 0)
                {
                    model.OlusturmaTarihi = DateTime.Now;
                    model.AdSoyad = ((model.Ad ?? "") + " " + (model.Soyad ?? "")).Trim();
                    model.SilindiMi = false;
                    _context.Musterilers.Add(model);
                    await _context.SaveChangesAsync();
                    musteriId = model.Id; // Yeni oluşan ID'yi al
                }
                else
                {
                    var entity = await _context.Musterilers.FindAsync(model.Id);
                    if (entity == null) return Json(new { success = false, message = "Müşteri bulunamadı!" });

                    entity.Ad = model.Ad; entity.Soyad = model.Soyad;
                    entity.AdSoyad = ((model.Ad ?? "") + " " + (model.Soyad ?? "")).Trim();
                    entity.FirmaAdi = model.FirmaAdi; entity.YetkiliKisi = model.YetkiliKisi;
                    entity.FirmaUnvani = model.FirmaUnvani; entity.VergiDairesi = model.VergiDairesi; entity.VergiNo = model.VergiNo;
                    entity.Telefon = model.Telefon; entity.IkinciTelefon = model.IkinciTelefon; entity.SabitTelefon = model.SabitTelefon;
                    entity.Eposta = model.Eposta;
                    entity.MusteriTipId = model.MusteriTipId; entity.MusteriDurumId = model.MusteriDurumId;
                    entity.Aciklama = model.Aciklama;
                    entity.GuncellemeTarihi = DateTime.Now;
                    _context.Musterilers.Update(entity);
                    await _context.SaveChangesAsync();
                }

                // Adres Kayıt İşlemi
                if (!string.IsNullOrEmpty(AcikAdres))
                {
                    var mevcutAdres = await _context.MusteriAdresleris.FirstOrDefaultAsync(a => a.MusteriId == musteriId);
                    if (mevcutAdres != null)
                    {
                        mevcutAdres.AdresTipi = AdresTipi ?? "Ev / İş";
                        mevcutAdres.Il = string.IsNullOrEmpty(Il) ? "Belirtilmedi" : Il;
                        mevcutAdres.AcikAdres = AcikAdres;
                        mevcutAdres.OlusturmaTarihi = DateTime.Now;
                        _context.MusteriAdresleris.Update(mevcutAdres);
                    }
                    else
                    {
                        _context.MusteriAdresleris.Add(new MusteriAdresleri
                        {
                            MusteriId = musteriId,
                            AdresTipi = AdresTipi ?? "Ev / İş",
                            Il = string.IsNullOrEmpty(Il) ? "Belirtilmedi" : Il,
                            AcikAdres = AcikAdres,
                            VarsayilanMi = true,
                            OlusturmaTarihi = DateTime.Now
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }

        [HttpGet]
        public async Task<JsonResult> GetMusteri(int id)
        {
            var m = await _context.Musterilers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            var a = await _context.MusteriAdresleris.AsNoTracking().FirstOrDefaultAsync(x => x.MusteriId == id);

            return Json(new { musteri = m, adres = a });
        }

        // FİNANSAL GEÇMİŞ (Sadece finans butonuna basıldığında)
        [HttpGet]
        public async Task<JsonResult> GetMusteriOdemeleri(int id)
        {
            var gecmis = await _context.KasaHareketleris
                .AsNoTracking()
                .Where(x => x.MusteriId == id)
                .OrderByDescending(x => x.Tarih)
                .Select(x => new {
                    x.Id,
                    Tarih = x.Tarih.HasValue ? x.Tarih.Value.ToString("dd.MM.yyyy HH:mm") : "-",
                    Tutar = x.Tutar ?? 0,
                    x.OdemeYontemi,
                    IslemTipi = x.IslemTipi ?? "Tahsilat"
                }).ToListAsync();
            return Json(gecmis);
        }

        [HttpPost]
        public async Task<JsonResult> Sil(int id)
        {
            var m = await _context.Musterilers.FindAsync(id);
            if (m == null) return Json(new { success = false });
            m.SilindiMi = true; m.PasifeAlinmaTarihi = DateTime.Now;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<JsonResult> NotEkle(int MusteriId, string NotIcerigi)
        {
            try
            {
                // MusteriNotlari modelin / tablon varsa onu kullan, yoksa veritabanındaki ismine göre değiştir
                var yeniNot = new MusteriNotlari
                {
                    MusteriId = MusteriId,
                    NotIcerigi = NotIcerigi,
                    Tarih = DateTime.Now
                };

                _context.MusteriNotlaris.Add(yeniNot);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Not eklenirken hata: " + ex.Message });
            }
        }
        [HttpPost]
        public async Task<JsonResult> CihazGorselYukle(int CihazId, IFormFile Gorsel)
        {
            try
            {
                if (Gorsel == null || Gorsel.Length == 0)
                    return Json(new { success = false, message = "Lütfen bir dosya seçin." });

                // Dosya yolunu belirle (wwwroot/uploads/cihazlar/)
                string klasorYolu = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "cihazlar");
                if (!Directory.Exists(klasorYolu)) Directory.CreateDirectory(klasorYolu);

                // Benzersiz dosya adı oluştur
                string dosyaAdi = Guid.NewGuid().ToString() + Path.GetExtension(Gorsel.FileName);
                string tamYol = Path.Combine(klasorYolu, dosyaAdi);

                using (var stream = new FileStream(tamYol, FileMode.Create))
                {
                    await Gorsel.CopyToAsync(stream);
                }

                // Veritabanına dosya yolunu kaydet (MusteriCihazlari tablosunda GorselYolu sütunu olduğunu varsayıyoruz)
                var cihaz = await _context.MusteriCihazlaris.FindAsync(CihazId);
                if (cihaz != null)
                {
                    cihaz.GorselYolu = "/uploads/cihazlar/" + dosyaAdi;
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, gorselUrl = cihaz.GorselYolu });
            }
            catch (Exception ex) { return Json(new { success = false, message = ex.Message }); }
        }
    }
}