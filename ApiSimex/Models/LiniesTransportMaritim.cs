using System;
using System.Collections.Generic;

namespace ApiSimex.Models;

public partial class LiniesTransportMaritim
{
    public int Id { get; set; }

    public string Nom { get; set; } = null!;

    public int CiutatId { get; set; }

    public virtual Ciutat Ciutat { get; set; } = null!;

    public virtual ICollection<Oferte> Ofertes { get; set; } = new List<Oferte>();
}
