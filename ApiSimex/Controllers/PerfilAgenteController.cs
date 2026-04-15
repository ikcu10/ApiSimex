using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiSimex.Models; // Tu namespace de modelos
using ApiSimex.ViewModels;   // Tu namespace de DTOs
using Microsoft.AspNetCore.Authorization;

namespace ApiSimex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PerfilAgenteController : ControllerBase
    {
        private readonly SimexContext _context; // ⚠️ CAMBIA ESTO por tu Contexto real

        public PerfilAgenteController(SimexContext context)
        {
            _context = context;
        }

        // 1. OBTENER EL PERFIL: GET api/PerfilAgent/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PerfilAgenteDTO>> GetPerfil(int id)
        {
            var usuari = await _context.Usuaris.FindAsync(id);

            if (usuari == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            // Opcional: Validar que sea un Agente Comercial (Asumiendo que su RolId es 3, cámbialo si es otro)
            // if (usuari.RolId != 3) return BadRequest("El usuario no es un agente comercial.");

            var perfilDto = new PerfilAgenteDTO
            {
                Id = usuari.Id,
                Nom = usuari.Nom,
                Cognoms = usuari.Cognoms,
                Correu = usuari.Correu,
                Idioma = usuari.Idioma
            };

            return Ok(perfilDto);
        }

        // 2. ACTUALIZAR DATOS PERSONALES: PUT api/PerfilAgent/dades/5
        [HttpPut("dades/{id}")]
        public async Task<IActionResult> ActualitzarDades(int id, ActualizarDatosAgenteDTO dto)
        {
            var usuari = await _context.Usuaris.FindAsync(id);
            if (usuari == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            // Actualizamos solo los campos permitidos
            usuari.Nom = dto.Nom;
            usuari.Cognoms = dto.Cognoms;
            usuari.Correu = dto.Correu;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Datos actualizados correctamente." });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "Error al guardar en la base de datos." });
            }
        }

        // 3. ACTUALIZAR IDIOMA: PUT api/PerfilAgent/idioma/5
        [HttpPut("idioma/{id}")]
        public async Task<IActionResult> ActualitzarIdioma(int id, ActualizarIdiomaAgenteDTO dto)
        {
            var usuari = await _context.Usuaris.FindAsync(id);
            if (usuari == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            usuari.Idioma = dto.Idioma;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Idioma actualizado correctamente." });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { message = "Error al guardar el idioma." });
            }
        }
    }
}
