using ApiSimex.Models;
using ApiSimex.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ApiSimex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PedidosComercialController : ControllerBase
    {
        private readonly SimexContext _context; // Tu DbContext

        // Inyectamos la base de datos a través del constructor
        public PedidosComercialController(SimexContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PedidoAgenteDTO>>> GetPedidos(
            [FromQuery] string? searchQuery,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // Leemos el ID directamente del Token de forma segura
            var idString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(idString))
            {
                return Unauthorized(new { mensaje = "Token inválido o sin ID de usuario" });
            }

            int agenteId = int.Parse(idString); // Convertimos el texto del Token a número entero

            // 1. Empezamos la consulta en la tabla de operaciones logísticas
            var query = _context.OperacionsLogistiques
                .Include(ol => ol.Oferta)
                    .ThenInclude(o => o.Client)
                        .ThenInclude(c => c.Usuari)
                .Include(ol => ol.Oferta.PortOrigen)
                    .ThenInclude(p => p.Ciutat)
                .Include(ol => ol.Oferta.PortDesti)
                    .ThenInclude(p => p.Ciutat)
                .Include(ol => ol.SeguimentOperacions)
                    .ThenInclude(so => so.TrackingStep)
                .AsQueryable();

            // 2. Filtramos por el Agente Comercial
            query = query.Where(ol => ol.Oferta.AgentComercialId == agenteId);

            // 3. Aplicamos el buscador (si el usuario escribió algo)
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(ol =>
                    ol.Id.ToString().Contains(searchQuery) ||
                    ol.Oferta.Client.Usuari.Nom.Contains(searchQuery));
            }

            // 4. Mapeamos al DTO y aplicamos Paginación para el RecyclerView
            var pedidos = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ol => new PedidoAgenteDTO
                {
                    Id = ol.Id,
                    PedidoCodigo = ol.Id.ToString(), // O usa ol.OfertaId.ToString() si prefieres
                    ClienteNombre = ol.Oferta.Client.Usuari.Nom,
                    Origen = ol.Oferta.PortOrigen.Ciutat.Nom,
                    Destino = ol.Oferta.PortDesti.Ciutat.Nom,

                    // Buscamos el último paso completado o en curso
                    PasoActualNombre = ol.SeguimentOperacions
                        .OrderByDescending(s => s.TrackingStepId)
                        .Where(s => s.EstatDelPas == "Completat" || s.EstatDelPas == "En curs")
                        .Select(s => s.TrackingStep.Nom)
                        .FirstOrDefault() ?? "Pendiente"
                })
                .ToListAsync();

            return Ok(pedidos);
        }

        // ENDPOINT: OBTENER EL DETALLE DEL SEGUIMIENTO (Para cargar la pantalla)

        [HttpGet("{pedidoId}/seguimiento")]
        public async Task<ActionResult<SeguimientoDetalleDTO>> GetSeguimientoPedido(int pedidoId)
        {
            // 1. Leer ID del agente comercial desde el Token
            var idString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idString)) return Unauthorized();
            int agenteId = int.Parse(idString);

            // 2. Buscar la operación, verificando que pertenece al agente logueado
            var operacion = await _context.OperacionsLogistiques
                .Include(ol => ol.Oferta)
                    .ThenInclude(o => o.Client)
                        .ThenInclude(c => c.Usuari)
                .Include(ol => ol.Oferta.PortOrigen)
                    .ThenInclude(p => p.Ciutat)
                .Include(ol => ol.Oferta.PortDesti)
                    .ThenInclude(p => p.Ciutat)
                .Include(ol => ol.SeguimentOperacions)
                    .ThenInclude(so => so.TrackingStep)
                .FirstOrDefaultAsync(ol => ol.Id == pedidoId && ol.Oferta.AgentComercialId == agenteId);

            if (operacion == null)
            {
                return NotFound(new { mensaje = "Pedido no encontrado o no tienes permiso para verlo." });
            }

            // 3. Mapear al DTO para enviarlo a Android
            var dto = new SeguimientoDetalleDTO
            {
                Id = operacion.Id,
                PedidoCodigo = operacion.Id.ToString(),
                ClienteNombre = operacion.Oferta.Client.Usuari.Nom,
                Origen = operacion.Oferta.PortOrigen.Ciutat.Nom,
                Destino = operacion.Oferta.PortDesti.Ciutat.Nom,
                // Ordenamos los pasos estrictamente por el campo 'Ordre'
                Pasos = operacion.SeguimentOperacions
                    .OrderBy(so => so.TrackingStep.Ordre)
                    .Select(so => new PasoSeguimientoDTO
                    {
                        TrackingStepId = so.TrackingStepId,
                        Nombre = so.TrackingStep.Nom,
                        Estado = so.EstatDelPas,
                        // CORRECCIÓN 1: Manejo de nulos en Ordre
                        Orden = so.TrackingStep.Ordre ?? 0
                    }).ToList()
            };

            return Ok(dto);
        }

        // ENDPOINT: AVANZAR AL SIGUIENTE PASO (Botón azul de la app)

        [HttpPost("{pedidoId}/siguiente-paso")]
        public async Task<IActionResult> AvanzarPaso(int pedidoId)
        {
            // 1. Leer ID del agente comercial desde el Token
            var idString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idString)) return Unauthorized();
            int agenteId = int.Parse(idString);

            // 2. Buscar la operación y sus pasos
            var operacion = await _context.OperacionsLogistiques
                .Include(ol => ol.Oferta)
                .Include(ol => ol.SeguimentOperacions)
                    .ThenInclude(so => so.TrackingStep)
                .FirstOrDefaultAsync(ol => ol.Id == pedidoId);

            if (operacion == null) return NotFound(new { mensaje = "Pedido no encontrado." });
            if (operacion.Oferta.AgentComercialId != agenteId) return Unauthorized(new { mensaje = "No autorizado." });

            // 3. Buscar cuál es el paso actual ("En curs")
            var pasoActual = operacion.SeguimentOperacions.FirstOrDefault(so => so.EstatDelPas == "En curs");

            if (pasoActual == null)
            {
                return BadRequest(new { mensaje = "No hay ningún paso 'En curs'. Puede que la operación ya esté finalizada." });
            }

            // 4. Marcar el paso actual como completado y ponerle la fecha de hoy
            pasoActual.EstatDelPas = "Completat";
            pasoActual.DataCompletat = DateTime.Now;

            // 5. Buscar cuál es el siguiente paso lógico basándonos en el 'Ordre'
            // CORRECCIÓN 2: Manejo de nulos en Ordre
            int ordenActual = pasoActual.TrackingStep.Ordre ?? 0;

            var pasoSiguiente = operacion.SeguimentOperacions
                .Where(so => so.TrackingStep.Ordre > ordenActual)
                .OrderBy(so => so.TrackingStep.Ordre)
                .FirstOrDefault();

            // 6. Activar el siguiente paso o finalizar la operación
            if (pasoSiguiente != null)                              
            {
                pasoSiguiente.EstatDelPas = "En curs";
            }
            else
            {
                // Era el último paso, así que rellenamos la DataFi de la operación general
                // CORRECCIÓN 3: Formateo de DateTime a DateOnly
                operacion.DataFi = DateOnly.FromDateTime(DateTime.Now);
            }

            // 7. Guardar todos los cambios en la base de datos de golpe
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Paso avanzado correctamente." });
        }
    }
}
