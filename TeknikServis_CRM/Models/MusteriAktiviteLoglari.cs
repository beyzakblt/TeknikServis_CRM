using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Gerekli
using System.ComponentModel.DataAnnotations.Schema; // Gerekli

namespace TeknikServis_CRM.Models;

public partial class MusteriAktiviteLoglari
{
    [Key] // Id'nin primary key olduğunu doğrula
    public long Id { get; set; }

    // Hata buradaydı: EF'e bu kolonun Musteriler tablosuna bağlı olduğunu söyleyelim
    public int MusteriId { get; set; }

    [ForeignKey("MusteriId")]
    public virtual Musteriler Musteri { get; set; }

    public int? KullaniciId { get; set; }

    public string IslemTipi { get; set; } = null!;

    public string? Aciklama { get; set; }

    public DateTime? Tarih { get; set; }
}