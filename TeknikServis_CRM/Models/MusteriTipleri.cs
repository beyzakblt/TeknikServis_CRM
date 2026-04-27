using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class MusteriTipleri
{
    public int Id { get; set; }

    public string TipAdi { get; set; } = null!;

    public string TipKodu { get; set; } = null!;

    public string? Aciklama { get; set; }

    public string? Durum { get; set; }
}
