using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeknikServis_CRM.Models;

[Table("IsEmirleri")]
public partial class IsEmirleri
{
    [Key]
    public int Id { get; set; }

    // DİKKAT: Veritabanında fiziksel olarak yoksa mutlaka [NotMapped] olmalı.
    [NotMapped]
    public int? ServisId { get; set; }

    [Column("MusteriId")]
    public int? MusteriId { get; set; }

    [Column("CihazId")]
    public int? CihazId { get; set; }

    [Column("TeknisyenId")]
    public int? TeknisyenId { get; set; }

    public string? ArizaAciklamasi { get; set; }

    public string? TeknikNot { get; set; }

    public string? Durum { get; set; } // Örn: Beklemede, Atölyede, Tamamlandı

    public decimal? ServisUcreti { get; set; }

    public DateTime? OlusturmaTarihi { get; set; } = DateTime.Now;

    // ==========================================
    // NAVIGASYON PROPERTY'LERİ (İlişkiler)
    // ==========================================

    [ForeignKey("MusteriId")]
    public virtual Musteriler? Musteri { get; set; }

    [ForeignKey("CihazId")]
    public virtual MusteriCihazlari? Cihaz { get; set; }

    [ForeignKey("TeknisyenId")]
    public virtual Kullanicilar? Teknisyen { get; set; }

    [NotMapped] // İlişkiyi de eşlem dışı bırakıyoruz çünkü ServisId DB'de yok
    [ForeignKey("ServisId")]
    public virtual ServisKayitlari? ServisKayit { get; set; }
}