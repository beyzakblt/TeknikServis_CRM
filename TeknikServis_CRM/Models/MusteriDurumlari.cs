using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class MusteriDurumlari
{
    public int Id { get; set; }

    public string DurumAdi { get; set; } = null!;

    public string DurumKodu { get; set; } = null!;

    public string? Aciklama { get; set; }

    public string? RenkKodu { get; set; }

    public string? Durum { get; set; }
}
