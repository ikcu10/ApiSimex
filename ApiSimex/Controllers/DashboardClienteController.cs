using ApiSimex.Models;
using ApiSimex.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace ApiSimex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardClienteController : ControllerBase
    {
        private readonly SimexContext _context; // Ajusta el nombre de tu contexto si es distinto

        public DashboardClienteController(SimexContext context)
        {
            _context = context;
        }

        // Método auxiliar privado para leer el "tatuaje" del Token (El ID del usuario)
        private int ObtenerUsuarioIdDelToken()
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier);
            return claimId != null ? int.Parse(claimId.Value) : 0;
        }

        // ==========================================
        // ENDPOINT 1: PERFIL DEL CLIENTE
        // ==========================================
        [HttpGet("Perfil")]
        public async Task<IActionResult> GetPerfil()
        {
            int usuariId = ObtenerUsuarioIdDelToken();

            var perfil = await _context.Usuaris
                .Where(u => u.Id == usuariId)
                .Select(u => new PerfilDTO
                {
                    Nom = u.Nom
                })
                .FirstOrDefaultAsync();

            if (perfil == null) return NotFound("Usuario no encontrado.");

            return Ok(perfil);
        }

        // ==========================================
        // ENDPOINT 2: ÚLTIMA PROPUESTA
        // ==========================================
        [HttpGet("UltimaPropuesta")]
        public async Task<IActionResult> GetUltimaPropuesta()
        {
            int usuariId = ObtenerUsuarioIdDelToken();

            // Buscamos la última oferta vinculada al client_id que a su vez pertenece a este usuariId
            var ultimaPropuesta = await _context.Ofertes
                .Where(o => o.Client.UsuariId == usuariId)
                .OrderByDescending(o => o.DataCreacio) // Ordenamos de más nueva a más vieja
                .Select(o => new UltimaPropuestaDTO
                {
                    NombreOperador = o.Operador.Nom, // Navegamos a la tabla usuaris a través de operador_id
                    Precio = o.Preu,
                    FechaInicio = o.DataValidessaInicial,
                    FechaCaducidad = o.DataValidessaFina
                })
                .FirstOrDefaultAsync(); // Solo cogemos la primera

            // Si devuelve null, Android recibe un 204 No Content (vacío), ideal para ocultar la vista
            if (ultimaPropuesta == null) return NoContent();

            return Ok(ultimaPropuesta);
        }

        // ==========================================
        // ENDPOINT 3: ÚLTIMOS ENVÍOS
        // ==========================================
        [HttpGet("UltimosEnvios")]
        public async Task<IActionResult> GetUltimosEnvios()
        {
            int usuariId = ObtenerUsuarioIdDelToken();

            var ultimosEnvios = await _context.OperacionsLogistiques
                .Where(ol => ol.Oferta.Client.UsuariId == usuariId)
                .OrderByDescending(ol => ol.Id) // Cogemos las operaciones más recientes
                .Take(5) // Limitamos a los 5 últimos para el RecyclerView
                .Select(ol => new EnvioResumenDTO
                {
                    Id = ol.Id,
                    CiudadOrigen = ol.Oferta.PortOrigenId != null ? ol.Oferta.PortOrigen.Ciutat.Nom :
                  (ol.Oferta.AeroportOrigenId != null ? ol.Oferta.AeroportOrigen.Ciutat.Nom : "N/A"),
                    CiudadDestino = ol.Oferta.PortDestiId != null ? ol.Oferta.PortDesti.Ciutat.Nom :
                   (ol.Oferta.AeroportDestiId != null ? ol.Oferta.AeroportDesti.Ciutat.Nom : "N/A"),

                    // LA NUEVA LÓGICA EXACTA QUE HAS PEDIDO:
                    PasoActual = ol.SeguimentOperacions
                .Where(so => so.EstatDelPas == "Completat") // 1. Filtramos solo los completados
                .OrderByDescending(so => so.DataCompletat)  // 2. Ordenamos por el que se completó más recientemente
                .Select(so => so.TrackingStep.Nom)          // 3. Sacamos el nombre de ese paso
                .FirstOrDefault() ?? "Pendiente"            // 4. Si aún no hay ninguno completado, dirá "Pendiente"
                })
                .ToListAsync();

            return Ok(ultimosEnvios);
        }

        // ==========================================
        // ENDPOINT: TODAS LAS PROPUESTAS (NOTIFICACIONES)
        // ==========================================
        [HttpGet("TodasLasPropuestas")]
        public async Task<IActionResult> GetTodasLasPropuestas()
        {
            int usuariId = ObtenerUsuarioIdDelToken();

            var propuestas = await _context.Ofertes
                .Where(o => o.Client.UsuariId == usuariId && o.VistPerClient == false) // <--- ¡TU IDEA!
                .OrderByDescending(o => o.DataCreacio) // Las más nuevas arriba
                .Select(o => new NotificacionOfertaDTO
                {
                    Id = o.Id,
                    NombreOperador = o.Operador.Nom,
                    FechaInicio = o.DataValidessaInicial,
                    FechaCaducidad = o.DataValidessaFina,
                    Precio = o.Preu
                })
                .ToListAsync();

            return Ok(propuestas);
        }

        // ==========================================
        // ENDPOINT: OBTENER DETALLE DE UNA PROPUESTA
        // ==========================================
        [HttpGet("DetallePropuesta/{id}")]
        public async Task<IActionResult> GetDetallePropuesta(int id)
        {
            var propuesta = await _context.Ofertes
                .Where(o => o.Id == id)
                .Select(o => new PropuestaDetalleDTO
                {
                    Id = o.Id,
                    OperadorLogistico = o.Operador.Nom,
                    FechaCaducidad = o.DataValidessaFina,
                    Origen = o.PortOrigenId != null
                        ? o.PortOrigen.Nom
                        : (o.AeroportOrigenId != null
                            ? o.AeroportOrigen.Nom
                            : "No definido"),
                    Destino = o.PortDestiId != null
                        ? o.PortDesti.Nom
                        : (o.AeroportDestiId != null
                            ? o.AeroportDesti.Nom
                            : "No definido"),
                    TipoTransporte = o.TipusTransport.Tipus,
                    // ❌ Antes: o.Incoterm.Nom  (incoterms no tiene 'nom')
                    // ✅ Ahora: hay que pasar por tipus_incoterms
                    Incoterm = o.Incoterm.TipusInconterm.Nom,
                    Peso = (double?)o.PesBrut,
                    Volumen = (double?)o.Volum,
                    // ❌ Antes: o.TipusFluxe.Nom  (tipus_fluxes no tiene 'nom')
                    // ✅ Ahora: el campo correcto es 'Tipus'
                    Flujo = o.TipusFluxe.Tipus,
                    Precio = o.Preu
                })
                .FirstOrDefaultAsync();

            if (propuesta == null) return NotFound("La propuesta no existe.");
            return Ok(propuesta);
        }

        // ==========================================
        // ENDPOINT: ACEPTAR PROPUESTA
        // ==========================================
        [HttpPut("AceptarPropuesta/{id}")]
        public async Task<IActionResult> AceptarPropuesta(int id)
        {
            var oferta = await _context.Ofertes.FindAsync(id);

            if (oferta == null) return NotFound();

            // Marcamos como gestionada para que desaparezca de "Notificaciones"
            oferta.VistPerClient = true;

            // Aquí podrías añadir: oferta.EstatId = 2; (Si tuvieras una tabla de estados para "Aceptada")

            await _context.SaveChangesAsync();
            return Ok(new { message = "Propuesta aceptada correctamente" });
        }

        // ==========================================
        // ENDPOINT: RECHAZAR PROPUESTA
        // ==========================================
        [HttpPut("RechazarPropuesta/{id}")]
        public async Task<IActionResult> RechazarPropuesta(int id, [FromBody] RechazoPropuestaDTO datosRechazo)
        {
            var oferta = await _context.Ofertes.FindAsync(id);

            if (oferta == null) return NotFound();

            // Marcamos como gestionada
            oferta.VistPerClient = true;

            // Guardamos la razón que el usuario escribió en la "ventana flotante" de Android
            oferta.RaoRebuig = datosRechazo.Razon;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Propuesta rechazada y motivo guardado" });
        }
    }
}
