using System;
using System.Collections.Generic;

namespace ApiSimex.Models;

public partial class EstatsOferte
{
    public int Id { get; set; }

    public string Estat { get; set; } = null!;

    public virtual ICollection<Oferte> Ofertes { get; set; } = new List<Oferte>();
}
