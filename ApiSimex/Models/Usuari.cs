using System;
using System.Collections.Generic;

namespace ApiSimex.Models;

public partial class Usuari
{
    public int Id { get; set; }

    public string Correu { get; set; } = null!;

    public string Contrasenya { get; set; } = null!;

    public string Nom { get; set; } = null!;

    public string Cognoms { get; set; } = null!;

    public int RolId { get; set; }

    public string? Idioma { get; set; }

    public virtual ICollection<Client> Clients { get; set; } = new List<Client>();

    public virtual ICollection<Oferte> OferteAgentComercials { get; set; } = new List<Oferte>();

    public virtual ICollection<Oferte> OferteOperadors { get; set; } = new List<Oferte>();

    public virtual Rol Rol { get; set; } = null!;
}
