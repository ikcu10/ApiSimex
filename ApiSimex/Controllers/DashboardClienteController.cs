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
        private readonly SimexContext _context;

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

        
        // ENDPOINT: PERFIL DEL CLIENTE
        
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

        
        // ENDPOINT 2: ÚLTIMA PROPUESTA
        
        [HttpGet("UltimaPropuesta")]
        public async Task<IActionResult> GetUltimaPropuesta()
        {
            int usuariId = ObtenerUsuarioIdDelToken();

            // Buscamos la última oferta vinculada al client_id
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
                .FirstOrDefaultAsync();

            if (ultimaPropuesta == null) return NoContent();

            return Ok(ultimaPropuesta);
        }

        
        // ENDPOINT 3: ÚLTIMOS ENVÍOS
        
        [HttpGet("UltimosEnvios")]
        public async Task<IActionResult> GetUltimosEnvios()
        {
            int usuariId = ObtenerUsuarioIdDelToken();

            var ultimosEnvios = await _context.OperacionsLogistiques
                .Where(ol => ol.Oferta.Client.UsuariId == usuariId)
                .OrderByDescending(ol => ol.Id)
                .Take(5) // Limitamos a los 5 últimos
                .Select(ol => new EnvioResumenDTO
                {
                    Id = ol.Id,
                    CiudadOrigen = ol.Oferta.PortOrigenId != null ? ol.Oferta.PortOrigen.Ciutat.Nom :
                  (ol.Oferta.AeroportOrigenId != null ? ol.Oferta.AeroportOrigen.Ciutat.Nom : "N/A"),
                    CiudadDestino = ol.Oferta.PortDestiId != null ? ol.Oferta.PortDesti.Ciutat.Nom :
                   (ol.Oferta.AeroportDestiId != null ? ol.Oferta.AeroportDesti.Ciutat.Nom : "N/A"),

                    
                    PasoActual = ol.SeguimentOperacions
                .Where(so => so.EstatDelPas == "Completat") // 1. Filtramos solo los completados
                .OrderByDescending(so => so.DataCompletat)  // 2. Ordenamos por el que se completó más recientemente
                .Select(so => so.TrackingStep.Nom)          // 3. Sacamos el nombre de ese paso
                .FirstOrDefault() ?? "Pendiente"            // 4. Si aún no hay ninguno completado, dirá "Pendiente"
                })
                .ToListAsync();

            return Ok(ultimosEnvios);
        }

        
        // ENDPOINT: TODAS LAS PROPUESTAS/OFERTAS (NOTIFICACIONES)
        
        [HttpGet("TodasLasPropuestas")]
        public async Task<IActionResult> GetTodasLasPropuestas()
        {
            int usuariId = ObtenerUsuarioIdDelToken();

            var propuestas = await _context.Ofertes
                .Where(o => o.Client.UsuariId == usuariId && o.VistPerClient == false)
                .OrderByDescending(o => o.DataCreacio)
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

        
        // ENDPOINT: OBTENER DETALLE DE UNA PROPUESTA/OFERTA
        
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
                    Incoterm = o.Incoterm.TipusInconterm.Nom,
                    Peso = (double?)o.PesBrut,
                    Volumen = (double?)o.Volum,
                    Flujo = o.TipusFluxe.Tipus,
                    Precio = o.Preu
                })
                .FirstOrDefaultAsync();

            if (propuesta == null) return NotFound("La propuesta no existe.");
            return Ok(propuesta);
        }

        
        // ENDPOINT: ACEPTAR PROPUESTA
        
        [HttpPut("AceptarPropuesta/{id}")]
        public async Task<IActionResult> AceptarPropuesta(int id)
        {
            var oferta = await _context.Ofertes.FindAsync(id);

            if (oferta == null) return NotFound();

            // Marcamos como gestionada para que desaparezca de "Notificaciones"
            oferta.VistPerClient = true;


            await _context.SaveChangesAsync();
            return Ok(new { message = "Propuesta aceptada correctamente" });
        }

        
        // ENDPOINT: RECHAZAR PROPUESTA
        
        [HttpPut("RechazarPropuesta/{id}")]
        public async Task<IActionResult> RechazarPropuesta(int id, [FromBody] RechazoPropuestaDTO datosRechazo)
        {
            var oferta = await _context.Ofertes.FindAsync(id);

            if (oferta == null) return NotFound();

            // Marcamos como gestionada
            oferta.VistPerClient = true;

            // Guardamos la razón que el usuario rechazo
            oferta.RaoRebuig = datosRechazo.Razon;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Propuesta rechazada y motivo guardado" });
        }

        
        // ENDPOINT: OBTENER PEDIDOS ACTIVOS (CON BUSCADOR)
        
        [HttpGet("PedidosActivos")]
        public async Task<IActionResult> GetPedidosActivos([FromQuery] string? busqueda = null)
        {
            int usuariId = ObtenerUsuarioIdDelToken();

            // 1. Empezamos la consulta filtrando por cliente y por "en curso" (data_fi == null)
            var query = _context.OperacionsLogistiques
                .Where(ol => ol.Oferta.Client.UsuariId == usuariId && ol.DataFi == null)
                .AsQueryable();

            // 2. Si el usuario de Android ha escrito algo en el buscador, aplicamos los filtros extra
            if (!string.IsNullOrEmpty(busqueda))
            {
                // Buscar por ID
                bool esNumero = int.TryParse(busqueda, out int idBuscado);

                query = query.Where(ol =>
                    // Busca por ID del pedido
                    (esNumero && ol.Id == idBuscado) ||
                    // O busca en el nombre de la ciudad de origen
                    (ol.Oferta.PortOrigen != null && ol.Oferta.PortOrigen.Ciutat.Nom.Contains(busqueda)) ||
                    (ol.Oferta.AeroportOrigen != null && ol.Oferta.AeroportOrigen.Ciutat.Nom.Contains(busqueda)) ||
                    // O busca en el nombre de la ciudad de destino
                    (ol.Oferta.PortDesti != null && ol.Oferta.PortDesti.Ciutat.Nom.Contains(busqueda)) ||
                    (ol.Oferta.AeroportDesti != null && ol.Oferta.AeroportDesti.Ciutat.Nom.Contains(busqueda))
                );
            }

            // 3. Preparamos los datos
            var pedidos = await query
                .Select(ol => new PedidoActivoDTO
                {
                    IdPedido = ol.Id,

                    // Calculamos el nombre de la ciudad Origen
                    CiudadOrigen = ol.Oferta.PortOrigenId != null ? ol.Oferta.PortOrigen.Ciutat.Nom :
                                  (ol.Oferta.AeroportOrigenId != null ? ol.Oferta.AeroportOrigen.Ciutat.Nom : "No definido"),

                    // Calculamos el nombre de la ciudad Destino
                    CiudadDestino = ol.Oferta.PortDestiId != null ? ol.Oferta.PortDesti.Ciutat.Nom :
                                   (ol.Oferta.AeroportDestiId != null ? ol.Oferta.AeroportDesti.Ciutat.Nom : "No definido"),

                    // Buscamos el último paso del tracking que esté completado
                    PasoActual = ol.SeguimentOperacions
                        .Where(so => so.EstatDelPas == "Completat" && so.DataCompletat != null)
                        .OrderByDescending(so => so.DataCompletat)
                        .Select(so => so.TrackingStep.Nom)
                        .FirstOrDefault() ?? "Pendiente"
                })
                .ToListAsync();

            return Ok(pedidos);
        }

        
        // ENDPOINT: HISTORIAL DE PEDIDOS (TERMINADOS)
        
        [HttpGet("HistorialPedidos")]
        public async Task<IActionResult> GetHistorialPedidos()
        {
            int usuariId = ObtenerUsuarioIdDelToken();

            var historial = await _context.OperacionsLogistiques
                // Filtramos por el usuario y aseguramos que DataFi NO sea nulo (está terminado)
                .Where(ol => ol.Oferta.Client.UsuariId == usuariId && ol.DataFi != null)
                .OrderByDescending(ol => ol.DataFi)
                .Select(ol => new HistorialPedidoDTO
                {
                    IdPedido = ol.Id,
                    // Reutilizamos lógica de puertos/aeropuertos
                    CiudadOrigen = ol.Oferta.PortOrigenId != null ? ol.Oferta.PortOrigen.Ciutat.Nom :
                                  (ol.Oferta.AeroportOrigenId != null ? ol.Oferta.AeroportOrigen.Ciutat.Nom : "No definido"),
                    CiudadDestino = ol.Oferta.PortDestiId != null ? ol.Oferta.PortDesti.Ciutat.Nom :
                                   (ol.Oferta.AeroportDestiId != null ? ol.Oferta.AeroportDesti.Ciutat.Nom : "No definido"),
                    FechaFinalizacion = ol.DataFi
                })
                .ToListAsync();

            return Ok(historial);
        }

        
        // ENDPOINT: LISTA DE DOCUMENTOS DEL CLIENTE
        
        [HttpGet("MisDocumentos")]
        public async Task<IActionResult> GetMisDocumentos()
        {
            int usuariId = ObtenerUsuarioIdDelToken();

            var documentos = await _context.Documents
                // Buscamos los documentos cuyo cliente esté vinculado a este usuario
                .Where(d => d.Client.UsuariId == usuariId)
                .OrderByDescending(d => d.DataCreacio)
                .Select(d => new DocumentoDTO
                {
                    IdDocumento = d.Id,
                    NombreDocumento = d.NomDocument,
                    FechaCreacion = d.DataCreacio
                })
                .ToListAsync();

            return Ok(documentos);
        }

        
        // ENDPOINT: VER/DESCARGAR EL DOCUMENTO FÍSICO
        
        [HttpGet("VerDocumento/{id}")]
        public async Task<IActionResult> GetVerDocumento(int id)
        {
            int usuariId = ObtenerUsuarioIdDelToken();

            // 1. Buscamos el documento y comprobamos que el d.Client.UsuariId sea el mismo que el del token.
            var documento = await _context.Documents
                .Include(d => d.Client) // Incluimos al cliente para poder verificar el ID
                .FirstOrDefaultAsync(d => d.Id == id && d.Client.UsuariId == usuariId);

            if (documento == null)
                return NotFound("Documento no encontrado o no tienes permiso para verlo.");

            // 2. Comprobamos que el archivo físico exista
            string rutaArchivo = documento.RutaArxiu;
            if (!System.IO.File.Exists(rutaArchivo))
                return NotFound("El archivo físico no existe en el servidor.");

            // 3. Averiguamos el tipo de archivo
            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(rutaArchivo, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            // 4. Devolvemos el archivo.
            return PhysicalFile(rutaArchivo, contentType);
        }

        
        // ENDPOINT: DETALLE Y TRACKING DE LA OPERACIÓN
        
        [HttpGet("Seguimiento/{idOperacion}")]
        public async Task<IActionResult> GetSeguimiento(int idOperacion)
        {
            int usuariId = ObtenerUsuarioIdDelToken();

            // 1. Buscamos la operación
            var operacion = await _context.OperacionsLogistiques
                .Include(ol => ol.Oferta)
                    .ThenInclude(o => o.Client)
                // Relaciones para origen y destino
                .Include(ol => ol.Oferta.PortOrigen).ThenInclude(p => p.Ciutat)
                .Include(ol => ol.Oferta.PortDesti).ThenInclude(p => p.Ciutat)
                .Include(ol => ol.Oferta.AeroportOrigen).ThenInclude(a => a.Ciutat)
                .Include(ol => ol.Oferta.AeroportDesti).ThenInclude(a => a.Ciutat)
                // Relación para Incoterm
                .Include(ol => ol.Oferta.Incoterm)
                    .ThenInclude(i => i.TipusInconterm)
                // Relaciones para el Timeline (Tracking)
                .Include(ol => ol.SeguimentOperacions)
                    .ThenInclude(so => so.TrackingStep)
                // Filtramos por ID de operación y aseguramos que es de este cliente
                .FirstOrDefaultAsync(ol => ol.Id == idOperacion && ol.Oferta.Client.UsuariId == usuariId);

            if (operacion == null)
            {
                return NotFound("Operación no encontrada o no tienes permiso para verla.");
            }

            // 3. Mapeamos la información extraída a nuestro DTO
            var dto = new SeguimientoOperacionDTO
            {
                IdOperacion = operacion.Id,
                // Lógica de Puertos vs Aeropuertos
                CiudadOrigen = operacion.Oferta.PortOrigenId != null ? operacion.Oferta.PortOrigen.Ciutat.Nom :
                              (operacion.Oferta.AeroportOrigenId != null ? operacion.Oferta.AeroportOrigen.Ciutat.Nom : "No definido"),
                CiudadDestino = operacion.Oferta.PortDestiId != null ? operacion.Oferta.PortDesti.Ciutat.Nom :
                               (operacion.Oferta.AeroportDestiId != null ? operacion.Oferta.AeroportDesti.Ciutat.Nom : "No definido"),

                // Extraemos el código del incoterm
                Incoterm = operacion.Oferta.Incoterm?.TipusInconterm?.Codi ?? "N/A",

                // Mapeamos y ordenamos la lista del tracking
                Tracking = operacion.SeguimentOperacions
                    .OrderBy(so => so.TrackingStep.Ordre)
                    .Select(so => new PasoTrackingDTO
                    {
                        NombrePaso = so.TrackingStep.Nom,
                        Estado = so.EstatDelPas,
                        FechaCompletado = so.DataCompletat,
                        Orden = so.TrackingStep.Ordre ?? 0
                    })
                    .ToList()
            };

            return Ok(dto);
        }

        [HttpGet("PerfilCliente")]
        public async Task<IActionResult> GetPerfilCliente()
        {
            int usuariId = ObtenerUsuarioIdDelToken();

            var perfil = await _context.Usuaris
                .Include(u => u.Clients)
                .Where(u => u.Id == usuariId)
                .Select(u => new PerfilClienteDTO
                {
                    Nombre = u.Nom,
                    Apellidos = u.Cognoms,
                    Correo = u.Correu,
                    Telefono = u.Clients.FirstOrDefault().Telefon ?? "",
                    Idioma = u.Idioma ?? ""
                })
                .FirstOrDefaultAsync();

            if (perfil == null) return NotFound("Usuario no encontrado.");
            return Ok(perfil);
        }

        [HttpPut("PerfilCliente")]
        public async Task<IActionResult> ActualizarPerfil([FromBody] PerfilClienteDTO datosActualizados)
        {
            int usuariId = ObtenerUsuarioIdDelToken();

            // Buscamos usuario y cliente
            var usuario = await _context.Usuaris.FindAsync(usuariId);
            var cliente = await _context.Clients.FirstOrDefaultAsync(c => c.UsuariId == usuariId);

            if (usuario == null || cliente == null) return NotFound();

            // Actualizamos datos
            usuario.Nom = datosActualizados.Nombre;
            usuario.Cognoms = datosActualizados.Apellidos;
            usuario.Correu = datosActualizados.Correo;
            usuario.Idioma = datosActualizados.Idioma;

            cliente.Telefon = datosActualizados.Telefono;

            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Perfil actualizado correctamente" });
        }
    }
}
