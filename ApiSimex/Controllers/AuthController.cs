using ApiSimex.Models;
using ApiSimex.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiSimex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SimexContext _context;
        private readonly IConfiguration _configuration;

        // Inyectamos la base de datos y la configuración
        public AuthController(SimexContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
        {
            // 1. Buscar al usuario por Email
            var usuario = await _context.Usuaris
                .FirstOrDefaultAsync(u => u.Correu == request.Email);

            if (usuario == null)
            {
                return Unauthorized(new { mensaje = "Email o contraseña incorrectos" });
            }

            // 2. Verificar la contraseña con BCrypt
            bool esPasswordValido = BCrypt.Net.BCrypt.Verify(request.Password, usuario.Contrasenya);

            if (!esPasswordValido)
            {
                return Unauthorized(new { mensaje = "Email o contraseña incorrectos" });
            }

            // 3. Creamos el Token JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            // Aquí "escondemos" datos dentro del token
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Correu),
                }),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtString = tokenHandler.WriteToken(token);

            // 4. Devolvemos el Token al móvil
            return Ok(new LoginResponseDTO
            {
                Token = jwtString,
                Mensaje = "Login exitoso",
                UsuarioId = usuario.Id,
                RolId = usuario.RolId
            });
        }

        [HttpGet("generar-hash")]
        public IActionResult GenerarHash(string textoPlano)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(textoPlano);

            return Ok(new
            {
                ContrasenyaOriginal = textoPlano,
                HashParaBaseDeDatos = hash
            });
        }
    }
}
