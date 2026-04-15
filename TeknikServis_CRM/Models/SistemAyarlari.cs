using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class SistemAyarlari
{
    public int Id { get; set; }

    public string? AyarAnahtari { get; set; }

    public string? AyarDegeri { get; set; }
}
