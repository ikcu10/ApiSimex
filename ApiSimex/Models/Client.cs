using System;
using System.Collections.Generic;

namespace ApiSimex.Models;

public partial class Client
{
    public int Id { get; set; }

    public int UsuariId { get; set; }

    public string? NomEmpresa { get; set; }

    public string? CifNif { get; set; }

    public string? Telefon { get; set; }

    public string? Adreca { get; set; }

    public string? RutaDniEncriptat { get; set; }

    public string? ClauAes { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<Oferte> Ofertes { get; set; } = new List<Oferte>();

    public virtual Usuari Usuari { get; set; } = null!;
}
