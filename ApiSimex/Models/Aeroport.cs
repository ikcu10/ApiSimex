using System;
using System.Collections.Generic;

namespace ApiSimex.Models;

public partial class Aeroport
{
    public int Id { get; set; }

    public string Codi { get; set; } = null!;

    public string Nom { get; set; } = null!;

    public int CiutatId { get; set; }

    public virtual Ciutat Ciutat { get; set; } = null!;

    public virtual ICollection<Oferte> OferteAeroportDestis { get; set; } = new List<Oferte>();

    public virtual ICollection<Oferte> OferteAeroportOrigens { get; set; } = new List<Oferte>();
}
