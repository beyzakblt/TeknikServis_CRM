using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class Roller
{
    public int Id { get; set; }

    public string RolAdi { get; set; } = null!;

    public string RolKodu { get; set; } = null!;

    public string Durum { get; set; } = null!;
}
