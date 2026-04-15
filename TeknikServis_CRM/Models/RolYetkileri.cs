using System;
using System.Collections.Generic;

namespace TeknikServis_CRM.Models;

public partial class RolYetkileri
{
    public int Id { get; set; }

    public int RolId { get; set; }

    public int YetkiId { get; set; }

    public bool EklemeVarMi { get; set; }

    public bool GuncellemeVarMi { get; set; }

    public bool SilmeVarMi { get; set; }

    public bool GoruntulemeVarMi { get; set; }
}
