using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class SifreSifirlamaTalepleri
{
    public int Id { get; set; }

    public int KullaniciId { get; set; }

    public string Kod { get; set; } = null!;

    public DateTime SonKullanimTarihi { get; set; }

    public bool KullanildiMi { get; set; }
}
