using System;
using System.Collections.Generic;

namespace ApiSimex.Models;

public partial class Oferte
{
    public int Id { get; set; }

    public int TipusTransportId { get; set; }

    public int TipusFluxeId { get; set; }

    public int TipusCarregaId { get; set; }

    public int IncotermId { get; set; }

    public int ClientId { get; set; }

    public string? Comentaris { get; set; }

    public int? AgentComercialId { get; set; }

    public int? TransportistaId { get; set; }

    public decimal? PesBrut { get; set; }

    public decimal? Volum { get; set; }

    public int TipusValidacioId { get; set; }

    public int? PortOrigenId { get; set; }

    public int? PortDestiId { get; set; }

    public int? AeroportOrigenId { get; set; }

    public int? AeroportDestiId { get; set; }

    public int? LiniaTransportMaritimId { get; set; }

    public int EstatOfertaId { get; set; }

    public int OperadorId { get; set; }

    public DateOnly DataCreacio { get; set; }

    public DateOnly? DataValidessaInicial { get; set; }

    public DateOnly? DataValidessaFina { get; set; }

    public string? RaoRebuig { get; set; }

    public int? TipusContenidorId { get; set; }

    public decimal? Preu { get; set; }

    public bool VistPerClient { get; set; }

    public virtual Aeroport? AeroportDesti { get; set; }

    public virtual Aeroport? AeroportOrigen { get; set; }

    public virtual Usuari? AgentComercial { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual EstatsOferte EstatOferta { get; set; } = null!;

    public virtual Incoterm Incoterm { get; set; } = null!;

    public virtual LiniesTransportMaritim? LiniaTransportMaritim { get; set; }

    public virtual ICollection<OperacionsLogistique> OperacionsLogistiques { get; set; } = new List<OperacionsLogistique>();

    public virtual Usuari Operador { get; set; } = null!;

    public virtual Port? PortDesti { get; set; }

    public virtual Port? PortOrigen { get; set; }

    public virtual TipusCarrega TipusCarrega { get; set; } = null!;

    public virtual TipusContenidor? TipusContenidor { get; set; }

    public virtual TipusFlux TipusFluxe { get; set; } = null!;

    public virtual TipusTransport TipusTransport { get; set; } = null!;

    public virtual TipusValidacion TipusValidacio { get; set; } = null!;

    public virtual Transportiste? Transportista { get; set; }
}
