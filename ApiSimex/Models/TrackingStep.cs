using System;
using System.Collections.Generic;

namespace ApiSimex.Models;

public partial class TrackingStep
{
    public int Id { get; set; }

    public int? Ordre { get; set; }

    public string? Nom { get; set; }

    public virtual ICollection<Incoterm> Incoterms { get; set; } = new List<Incoterm>();

    public virtual ICollection<SeguimentOperacion> SeguimentOperacions { get; set; } = new List<SeguimentOperacion>();
}
