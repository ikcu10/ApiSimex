using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using ApiSimex.Models; // Ajusta esto a tu namespace de base de datos
using Microsoft.EntityFrameworkCore;

namespace ApiSimex.Servicios
{
    public class DniSocketServer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly int _puerto = 11000;
        private readonly string _rutaCarpetaDni = @"C:\archivos_simex\dnis"; // Cambia esta ruta según tu PC

        public DniSocketServer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
            if (!Directory.Exists(_rutaCarpetaDni)) Directory.CreateDirectory(_rutaCarpetaDni);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, _puerto);
            listener.Start();
            Console.WriteLine($"[SOCKETS] Servidor DNI escuchando en el puerto {_puerto}");

            while (!stoppingToken.IsCancellationRequested)
            {
                // Esperamos a que un móvil Android se conecte
                TcpClient client = await listener.AcceptTcpClientAsync(stoppingToken);

                // Creamos un THREAD independiente para cumplir con el requisito del profesor
                Thread clientThread = new Thread(() => ManejarCliente(client));
                clientThread.IsBackground = true;
                clientThread.Start();
            }
        }

        private void ManejarCliente(TcpClient client)
        {
            try
            {
                using NetworkStream stream = client.GetStream();
                using StreamReader reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
                using BinaryReader binReader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);

                // 1. Leemos el comando inicial (Ej: "UPLOAD|5" o "DOWNLOAD|5")
                string comandoInicial = reader.ReadLine();
                if (string.IsNullOrEmpty(comandoInicial)) return;

                string[] partes = comandoInicial.Split('|');
                string accion = partes[0];
                int usuariId = int.Parse(partes[1]);

                // Necesitamos un Scope para usar la base de datos dentro de este Thread
                using var scope = _scopeFactory.CreateScope();
                var _context = scope.ServiceProvider.GetRequiredService<SimexContext>(); // Ajusta el nombre de tu Context

                var clienteDb = _context.Clients.FirstOrDefault(c => c.UsuariId == usuariId);
                if (clienteDb == null) return;

                if (accion == "UPLOAD")
                {
                    // 2. Generamos clave AES
                    using Aes aes = Aes.Create();
                    aes.GenerateKey();
                    aes.GenerateIV();
                    string clauBase64 = Convert.ToBase64String(aes.Key);

                    // 3. Leemos el archivo enviado por el móvil
                    // (El móvil debe enviar el tamaño del archivo primero, luego los bytes)
                    int longitudArchivo = binReader.ReadInt32();
                    byte[] archivoBytes = binReader.ReadBytes(longitudArchivo);

                    // 4. Encriptamos
                    byte[] encriptado = EncriptarAES(archivoBytes, aes.Key, aes.IV);

                    // 5. Guardamos en disco duro
                    string rutaArchivo = Path.Combine(_rutaCarpetaDni, $"dni_usu_{usuariId}.dat");

                    // Guardamos el IV al principio del archivo para poder desencriptar luego
                    using (FileStream fs = new FileStream(rutaArchivo, FileMode.Create))
                    {
                        fs.Write(aes.IV, 0, aes.IV.Length);
                        fs.Write(encriptado, 0, encriptado.Length);
                    }

                    // 6. Actualizamos base de datos
                    clienteDb.RutaDniEncriptat = rutaArchivo;
                    clienteDb.ClauAes = clauBase64;
                    _context.SaveChanges();

                    Console.WriteLine($"[SOCKETS] DNI subido y encriptado para usuario {usuariId}");
                }
                else if (accion == "DOWNLOAD")
                {
                    if (string.IsNullOrEmpty(clienteDb.RutaDniEncriptat) || string.IsNullOrEmpty(clienteDb.ClauAes))
                        return; // No hay DNI

                    // 2. Leemos la clave AES de la BD y el archivo del disco
                    byte[] claveKey = Convert.FromBase64String(clienteDb.ClauAes);
                    byte[] iv = new byte[16];
                    byte[] archivoEncriptado;

                    using (FileStream fs = new FileStream(clienteDb.RutaDniEncriptat, FileMode.Open))
                    {
                        fs.Read(iv, 0, iv.Length); // Sacamos el IV que guardamos al principio
                        archivoEncriptado = new byte[fs.Length - iv.Length];
                        fs.Read(archivoEncriptado, 0, archivoEncriptado.Length);
                    }

                    // 3. Desencriptamos
                    byte[] archivoOriginal = DesencriptarAES(archivoEncriptado, claveKey, iv);

                    // 4. Se lo enviamos al móvil Android
                    using BinaryWriter binWriter = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
                    binWriter.Write(archivoOriginal.Length); // Mandamos tamaño
                    binWriter.Write(archivoOriginal);        // Mandamos bytes

                    Console.WriteLine($"[SOCKETS] DNI desencriptado y enviado a usuario {usuariId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SOCKETS ERROR]: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }

        // --- MÉTODOS DE ENCRIPTACIÓN AES ---
        private byte[] EncriptarAES(byte[] datosLimpios, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            using MemoryStream ms = new MemoryStream();
            using CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(datosLimpios, 0, datosLimpios.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }

        private byte[] DesencriptarAES(byte[] datosEncriptados, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            using MemoryStream ms = new MemoryStream();
            using CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(datosEncriptados, 0, datosEncriptados.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }
    }
}