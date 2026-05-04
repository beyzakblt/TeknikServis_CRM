using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TeknikServis_CRM.Models;

[Table("MusteriCihazlari")] // Veritabanındaki tablo adının bu olduğundan emin olur
public partial class MusteriCihazlari
{
    [Key] // Id alanının Primary Key olduğunu belirtir
    public int Id { get; set; }

    [Column("MusteriId")] // SQL'de kolon adı MusteriId ise burayı mühürler
    public int MusteriId { get; set; }

    [Column("CihazTipId")] // SQL'de kolon adı CihazTipId ise burayı mühürler
    public int CihazTipId { get; set; }

    public string? Marka { get; set; }

    public string? Model { get; set; }

    public string? SeriNo { get; set; }

    public string? GarantiDurumu { get; set; }

    public DateTime? KurulumTarihi { get; set; }

    public bool? AktifMi { get; set; }
  

    // ============================================================
    // NAVIGASYON PROPERTY'LERİ
    // (EF'in tablolar arası ilişki kurmasını sağlar)
    // ============================================================

    [ForeignKey("MusteriId")]
    public virtual Musteriler? Musteri { get; set; }

    [ForeignKey("CihazTipId")]
    public virtual CihazTipleri? CihazTip { get; set; }
}