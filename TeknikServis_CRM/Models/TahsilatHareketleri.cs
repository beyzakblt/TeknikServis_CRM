using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class TahsilatHareketleri
{
    public int Id { get; set; }

    public int MusteriId { get; set; }

    public int? ServisKaydiId { get; set; }

    public string HareketTipi { get; set; } = null!;

    public decimal Tutar { get; set; }

    public string? OdemeYontemi { get; set; }

    public DateTime? IslemTarihi { get; set; }
}
