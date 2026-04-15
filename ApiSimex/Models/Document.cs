using System;
using System.Collections.Generic;

namespace ApiSimex.Models;

public partial class Document
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public string NomDocument { get; set; } = null!;

    public string RutaArxiu { get; set; } = null!;

    public DateTime DataCreacio { get; set; }

    public virtual Client Client { get; set; } = null!;
}
