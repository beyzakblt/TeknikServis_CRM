using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class ServisDurumlari
{
    public int Id { get; set; }

    public string DurumAdi { get; set; } = null!;

    public string DurumKodu { get; set; } = null!;

    public string? RenkKodu { get; set; }
}
