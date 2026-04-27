using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class CihazTipleri
{
    public int Id { get; set; }

    public string TipAdi { get; set; } = null!;

    public string TipKodu { get; set; } = null!;
}
