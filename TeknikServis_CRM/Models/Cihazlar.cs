using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class Cihazlar
{
    public int Id { get; set; }

    public int? MusteriId { get; set; }

    public string? Marka { get; set; }

    public string? Model { get; set; }

    public string? SeriNo { get; set; }

    public string? CihazNotu { get; set; }
}
