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

        
        // ENDPOINT: RESUMEN DEL DASHBOARD (Las 3 tarjetas)
        
        [HttpGet("resumen/{agentId}")]
        public async Task<ActionResult<ResumenDashboardDTO>> GetResumen(int agentId)
        {
            // 1. Buscamos todas las operaciones logísticas que nacen de ofertas de este agente
            var operacionesDelAgente = _context.OperacionsLogistiques
                .Include(op => op.Oferta)
                .Where(op => op.Oferta.AgentComercialId == agentId);

            // 2. Calculamos los 3 números directamente en la base de datos
            var totalOperaciones = await operacionesDelAgente.CountAsync();

            var totalClientes = await operacionesDelAgente
                .Select(op => op.Oferta.ClientId)
                .Distinct()
                .CountAsync();

            var operacionesEnCurso = await operacionesDelAgente
                .Where(op => op.DataFi == null)
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

        
        // ENDPOINT: CONTEXTO LOGÍSTICO (Alertas Globales)
        
        [HttpGet("alertas")]
        public async Task<ActionResult<IEnumerable<AlertaGlobalDTO>>> GetAlertasGlobals()
        {
            // Leemos toda la tabla y la mapeamos al DTO
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
