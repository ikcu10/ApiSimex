namespace ApiSimex.ViewModels
{
    public class LoginResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public int UsuarioId { get; set; }
        public int RolId { get; set; }
    }
}
