using System;
using System.Collections.Generic;

namespace ApiSimex.Models;

public partial class AlertesGlobal
{
    public int Id { get; set; }

    public string Titol { get; set; } = null!;

    public string Descripcio { get; set; } = null!;

    public string Tipus { get; set; } = null!;
}
