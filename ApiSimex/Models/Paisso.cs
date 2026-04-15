using System;
using System.Collections.Generic;

namespace ApiSimex.Models;

public partial class Paisso
{
    public int Id { get; set; }

    public string Nom { get; set; } = null!;

    public virtual ICollection<Ciutat> Ciutats { get; set; } = new List<Ciutat>();
}
