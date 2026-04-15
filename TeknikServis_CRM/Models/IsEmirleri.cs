using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class IsEmirleri
{
    public int Id { get; set; }

    public int? CihazId { get; set; }

    public int? MusteriId { get; set; }

    public int? TeknisyenId { get; set; }

    public string? ArizaAciklamasi { get; set; }

    public string? TeknikNot { get; set; }

    public string? Durum { get; set; }

    public decimal? ServisUcreti { get; set; }

    public DateTime? OlusturmaTarihi { get; set; }
}
