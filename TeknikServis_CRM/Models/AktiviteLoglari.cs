using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class AktiviteLoglari
{
    public long Id { get; set; }

    public int? KullaniciId { get; set; }

    public string IslemTipi { get; set; } = null!;

    public string ModulAdi { get; set; } = null!;

    public DateTime Tarih { get; set; }
}
