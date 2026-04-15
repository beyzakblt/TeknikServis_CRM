using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class Musteriler
{
    public int Id { get; set; }

    public string? Ad { get; set; }

    public string? Soyad { get; set; }

    public string? Telefon { get; set; }

    public string? Eposta { get; set; }

    public string? Adres { get; set; }

    public decimal? CariBakiye { get; set; }

    public string? Sehir { get; set; }

    public string? Ilce { get; set; }

    public DateTime? KayitTarihi { get; set; }
}
