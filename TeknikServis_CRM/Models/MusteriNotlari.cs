using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class MusteriNotlari
{
    public int Id { get; set; }

    public int MusteriId { get; set; }

    public int NotTipId { get; set; }

    public string? Baslik { get; set; }

    public string NotIcerigi { get; set; } = null!;

    public int? EkleyenKullaniciId { get; set; }

    public DateTime? Tarih { get; set; }

    public bool? GizliMi { get; set; }
}
