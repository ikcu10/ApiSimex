using System;
using System.Collections.Generic;

namespace ApiSimex.Models;

public partial class TipusContenidor
{
    public int Id { get; set; }

    public string Tipus { get; set; } = null!;

    public virtual ICollection<Oferte> Ofertes { get; set; } = new List<Oferte>();
}
