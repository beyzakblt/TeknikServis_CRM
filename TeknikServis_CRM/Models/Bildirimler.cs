using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class Bildirimler
{
    public int Id { get; set; }

    public int? KullaniciId { get; set; }

    public string? Mesaj { get; set; }

    public bool? OkunduMu { get; set; }

    public DateTime? Tarih { get; set; }
}
