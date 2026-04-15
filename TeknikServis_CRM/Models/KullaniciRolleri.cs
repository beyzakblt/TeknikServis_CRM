using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class KullaniciRolleri
{
    public int Id { get; set; }

    public int KullaniciId { get; set; }

    public int RolId { get; set; }

    public DateTime AtanmaTarihi { get; set; }
}
