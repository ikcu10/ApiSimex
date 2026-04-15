using System;
using System.Collections.Generic;

namespace ApiSimex.Models;

public partial class Incoterm
{
    public int Id { get; set; }

    public int TipusIncontermId { get; set; }

    public int TrackingStepsId { get; set; }

    public virtual ICollection<Oferte> Ofertes { get; set; } = new List<Oferte>();

    public virtual TipusIncoterm TipusInconterm { get; set; } = null!;

    public virtual TrackingStep TrackingSteps { get; set; } = null!;
}
