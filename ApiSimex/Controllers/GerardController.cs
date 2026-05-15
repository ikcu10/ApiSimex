using ApiSimex.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiSimex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GerardController : ControllerBase
    {
        private readonly SimexContext _context;

        // Inyectamos la base de datos
        public GerardController(SimexContext context)
        {
            _context = context;
        }

        // 1. GET: Leer una tabla y su relación (JOIN)
        [HttpGet("Puertos")]
        public async Task<IActionResult> GetPuertos()
        {
            // Devuelve el modelo Port puro y añade el modelo Ciutat
            var puertos = await _context.Ports
                .Include(p => p.Ciutat)
                .ToListAsync();

            if (!puertos.Any()) return NotFound("No hay puertos.");

            return Ok(puertos);
        }

        // 2. GET POR ID: Filtrar una tabla hija por el ID del padre
        [HttpGet("Operaciones/{idOperacion}/Tracking")]
        public async Task<IActionResult> GetTrackingOperacion(int idOperacion)
        {
            var tracking = await _context.SeguimentOperacions
                .Include(s => s.TrackingStep)
                .Where(s => s.OperacioId == idOperacion)
                .OrderBy(s => s.TrackingStep.Ordre)
                .ToListAsync();

            if (!tracking.Any()) return NotFound("No hay tracking para esta operación.");

            return Ok(tracking);
        }

        // 3. POST: Insertar un registro nuevo
        [HttpPost("Alertas")]
        public async Task<IActionResult> CrearAlertaGlobal([FromBody] AlertesGlobal alerta)
        {
            // EF Core recibe el modelo desde el JSON y lo prepara
            _context.AlertesGlobals.Add(alerta);

            // Lo inserta físicamente en SQL Server
            await _context.SaveChangesAsync();

            // Devuelve el objeto ya con su ID generado
            return Ok(alerta);
        }

        // 4. PUT: Actualizar un registro existente
        [HttpPut("Operaciones/{idOperacion}/AvanzarPaso")]
        public async Task<IActionResult> AvanzarPasoTracking(int idOperacion)
        {
            // 1. Buscamos el registro en la base de datos
            var pasoPendiente = await _context.SeguimentOperacions
                .Where(s => s.OperacioId == idOperacion && s.EstatDelPas == "Pendent")
                .OrderBy(s => s.TrackingStep.Ordre)
                .FirstOrDefaultAsync();

            if (pasoPendiente == null) return BadRequest("No hay pasos pendientes.");

            // 2. Modificamos las propiedades del modelo
            pasoPendiente.EstatDelPas = "Completat";
            pasoPendiente.DataCompletat = DateTime.Now;

            // 3. Guardamos los cambios
            await _context.SaveChangesAsync();

            return Ok(pasoPendiente);
        }
    }
}
