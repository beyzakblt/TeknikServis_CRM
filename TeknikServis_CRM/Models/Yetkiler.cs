using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class Yetkiler
{
    public int Id { get; set; }

    public string YetkiAdi { get; set; } = null!;

    public string YetkiKodu { get; set; } = null!;

    public string ModulAdi { get; set; } = null!;

    public string Durum { get; set; } = null!;
}
