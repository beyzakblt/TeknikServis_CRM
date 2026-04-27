using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class MusteriNotTipleri
{
    public int Id { get; set; }

    public string NotTipiAdi { get; set; } = null!;

    public string NotTipiKodu { get; set; } = null!;
}
