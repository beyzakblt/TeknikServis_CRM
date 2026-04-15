using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class Kullanicilar
{
    public int Id { get; set; }

    public string Ad { get; set; } = null!;

    public string Soyad { get; set; } = null!;

    public string? AdSoyad { get; set; }

    public string KullaniciAdi { get; set; } = null!;

    public string Eposta { get; set; } = null!;

    public string? Telefon { get; set; }

    public string SifreHash { get; set; } = null!;

    public string? ProfilResmi { get; set; }

    public string Durum { get; set; } = null!;

    public DateTime? SonGirisTarihi { get; set; }

    public DateTime OlusturmaTarihi { get; set; }

    public int? OlusturanKullaniciId { get; set; }

    public bool SilindiMi { get; set; }

    public string? Sifre { get; set; }

    public string? OnayKodu { get; set; }

    public bool IsOnayli { get; set; }
}
