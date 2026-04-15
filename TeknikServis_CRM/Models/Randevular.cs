using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class Randevular
{
    public int Id { get; set; }

    public int? MusteriId { get; set; }

    public int? IsEmriId { get; set; }

    public DateTime? RandevuTarihi { get; set; }

    public string? Aciklama { get; set; }
}
