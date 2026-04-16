using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiSimex.Models;
using ApiSimex.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace ApiSimex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PerfilAgenteController : ControllerBase
    {
        private readonly SimexContext _context;

        public PerfilAgenteController(SimexContext context)
        {
            _context = context;
        }

        // ENDPOINT: OBTENER EL PERFIL
        [HttpGet("{id}")]
        public async Task<ActionResult<PerfilAgenteDTO>> GetPerfil(int id)
        {
            var usuari = await _context.Usuaris.FindAsync(id);

            if (usuari == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

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

        //ENDPOINT: ACTUALIZAR DATOS PERSONALES
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

        //ENDPOINT: ACTUALIZAR IDIOMA
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
