using System;
using System.Collections.Generic;

namespace ApiSimex.Models;

public partial class SeguimentOperacion
{
    public int Id { get; set; }

    public int OperacioId { get; set; }

    public int TrackingStepId { get; set; }

    public string EstatDelPas { get; set; } = null!;

    public DateTime? DataCompletat { get; set; }

    public virtual OperacionsLogistique Operacio { get; set; } = null!;

    public virtual TrackingStep TrackingStep { get; set; } = null!;
}
