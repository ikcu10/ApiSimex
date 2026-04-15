using System;
using System.Collections.Generic;

namespace ApiSimex.Models;

public partial class Rol
{
    public int Id { get; set; }

    public string Rol1 { get; set; } = null!;

    public virtual ICollection<Usuari> Usuaris { get; set; } = new List<Usuari>();
}
