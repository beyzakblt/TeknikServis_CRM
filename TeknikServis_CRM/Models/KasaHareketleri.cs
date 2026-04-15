using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class KasaHareketleri
{
    public int Id { get; set; }

    public int? IsEmriId { get; set; }

    public int? MusteriId { get; set; }

    public decimal? Tutar { get; set; }

    public string? IslemTipi { get; set; }

    public string? OdemeYontemi { get; set; }

    public DateTime? Tarih { get; set; }
}
