using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class MusteriBelgeleri
{
    public int Id { get; set; }

    public int MusteriId { get; set; }

    public string BelgeAdi { get; set; } = null!;

    public string DosyaYolu { get; set; } = null!;

    public DateTime? YuklemeTarihi { get; set; }
}
