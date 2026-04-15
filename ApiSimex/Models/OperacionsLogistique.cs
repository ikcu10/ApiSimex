using System;
using System.Collections.Generic;

namespace ApiSimex.Models;

public partial class OperacionsLogistique
{
    public int Id { get; set; }

    public int OfertaId { get; set; }

    public DateOnly? DataInici { get; set; }

    public DateOnly? DataFi { get; set; }

    public virtual Oferte Oferta { get; set; } = null!;

    public virtual ICollection<SeguimentOperacion> SeguimentOperacions { get; set; } = new List<SeguimentOperacion>();
}
