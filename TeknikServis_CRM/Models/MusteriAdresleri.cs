using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class MusteriAdresleri
{
    public int Id { get; set; }

    public int MusteriId { get; set; }

    public string? AdresTipi { get; set; }

    public string Il { get; set; } = null!;

    public string? Ilce { get; set; }

    public string? Mahalle { get; set; }

    public string AcikAdres { get; set; } = null!;

    public string? KonumNotu { get; set; }

    public string? AdresTarifi { get; set; }

    public bool? VarsayilanMi { get; set; }

    public string? Durum { get; set; }

    public DateTime? OlusturmaTarihi { get; set; }
}
