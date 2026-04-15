using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class KullaniciOturumlari
{
    public int Id { get; set; }

    public int KullaniciId { get; set; }

    public string Token { get; set; } = null!;

    public string? IpAdresi { get; set; }

    public DateTime GirisTarihi { get; set; }

    public bool AktifMi { get; set; }
}
