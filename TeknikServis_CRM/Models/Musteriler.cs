using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // BUNU KESİNLİKLE EKLE

namespace TeknikServis_CRM.Models;

public partial class Musteriler
{
    public int Id { get; set; }
    public int MusteriTipId { get; set; }
    public int MusteriDurumId { get; set; }
    public string? Ad { get; set; }
    public string? Soyad { get; set; }
    public string? AdSoyad { get; set; }
    public string? FirmaAdi { get; set; }
    public string? YetkiliKisi { get; set; }
    public string Telefon { get; set; } = null!;
    public string? IkinciTelefon { get; set; }
    public string? Eposta { get; set; }
    public string? VergiDairesi { get; set; }
    public string? VergiNo { get; set; }
    public string? FirmaUnvani { get; set; }
    public string? SabitTelefon { get; set; }
    public string? KaynakBilgisi { get; set; }
    public string? Aciklama { get; set; }
    public DateTime? SonServisTarihi { get; set; }
    public DateTime? PasifeAlinmaTarihi { get; set; }
    public string? PasifeAlinmaNedeni { get; set; }
    public DateTime? OlusturmaTarihi { get; set; }
    public DateTime? GuncellemeTarihi { get; set; }
    public int? OlusturanKullaniciId { get; set; }
    public int? GuncelleyenKullaniciId { get; set; }
    public bool SilindiMi { get; set; }

    // ================================
    // NAVIGATION (TEKİL)
    // ================================

    public virtual MusteriTipleri? MusteriTip { get; set; }
    public virtual MusteriDurumlari? MusteriDurum { get; set; }

    // ================================
    // COLLECTIONS (Sadece listeler kalacak, [ForeignKey] etiketlerini sildik)
    // ================================

    // ================================
    // COLLECTIONS (DÜZELTİLMİŞ KISIM)
    // ================================

    // Alt modelde ilişki tanımı OLMAYANLAR (MusterilerId hatasını bunlar veriyordu, çözüldü)
    [System.ComponentModel.DataAnnotations.Schema.ForeignKey("MusteriId")]
    public virtual ICollection<MusteriAdresleri> MusteriAdresleri { get; set; } = new List<MusteriAdresleri>();

    [System.ComponentModel.DataAnnotations.Schema.ForeignKey("MusteriId")]
    public virtual ICollection<MusteriNotlari> MusteriNotlari { get; set; } = new List<MusteriNotlari>();

    [System.ComponentModel.DataAnnotations.Schema.ForeignKey("MusteriId")]
    public virtual ICollection<MusteriBelgeleri> MusteriBelgeleri { get; set; } = new List<MusteriBelgeleri>();

    // Alt modelde kendi ilişkisi ZATEN OLANLAR (Dokunmuyoruz ki çift tanımlama hatası vermesin)
    public virtual ICollection<MusteriAktiviteLoglari> MusteriAktiviteLoglari { get; set; } = new List<MusteriAktiviteLoglari>();
    public virtual ICollection<MusteriEtiketAtamalari> MusteriEtiketAtamalari { get; set; } = new List<MusteriEtiketAtamalari>();
    public virtual ICollection<MusteriCihazlari> MusteriCihazlari { get; set; } = new List<MusteriCihazlari>();
}