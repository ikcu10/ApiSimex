using ApiSimex.Models;
using ApiSimex.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace ApiSimex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardAgenteController : ControllerBase
    {
        private readonly SimexContext _context;

        public DashboardAgenteController(SimexContext context)
        {
            _context = context;
        }

        // ====================================================================
        // ENDPOINT 1: RESUMEN DEL DASHBOARD (Las 3 tarjetas)
        // GET: api/DashboardAgent/resumen/5 (donde 5 es el ID del agente)
        // ====================================================================
        [HttpGet("resumen/{agentId}")]
        public async Task<ActionResult<ResumenDashboardDTO>> GetResumen(int agentId)
        {
            // 1. Buscamos todas las operaciones logísticas que nacen de ofertas de este agente
            var operacionesDelAgente = _context.OperacionsLogistiques
                .Include(op => op.Oferta) // Incluimos la oferta para poder leer el AgentComercialId y el ClientId
                .Where(op => op.Oferta.AgentComercialId == agentId);

            // 2. Calculamos los 3 números mágicos directamente en la base de datos
            var totalOperaciones = await operacionesDelAgente.CountAsync();

            var totalClientes = await operacionesDelAgente
                .Select(op => op.Oferta.ClientId)
                .Distinct() // Distinct asegura que no contemos al mismo cliente dos veces
                .CountAsync();

            var operacionesEnCurso = await operacionesDelAgente
                .Where(op => op.DataFi == null) // La genialidad que se te ocurrió: data_fi es nulo
                .CountAsync();

            // 3. Montamos el DTO y lo enviamos
            var resumen = new ResumenDashboardDTO
            {
                TotalOperaciones = totalOperaciones,
                TotalClientes = totalClientes,
                OperacionesEnCurso = operacionesEnCurso
            };

            return Ok(resumen);
        }

        // ====================================================================
        // ENDPOINT 2: CONTEXTO LOGÍSTICO (Alertas Globales)
        // GET: api/DashboardAgent/alertas
        // ====================================================================
        [HttpGet("alertas")]
        public async Task<ActionResult<IEnumerable<AlertaGlobalDTO>>> GetAlertasGlobals()
        {
            // Simplemente leemos toda la tabla aislada y la mapeamos al DTO
            var alertas = await _context.AlertesGlobals
                .Select(a => new AlertaGlobalDTO
                {
                    Id = a.Id,
                    Titol = a.Titol,
                    Descripcio = a.Descripcio,
                    Tipus = a.Tipus
                })
                .ToListAsync();

            return Ok(alertas);
        }
    }
}
