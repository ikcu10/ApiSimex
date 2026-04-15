namespace ApiSimex.ViewModels
{
    public class PerfilAgenteDTO
    {
        public int Id { get; set; }
        public string Nom { get; set; } = null!;
        public string Cognoms { get; set; } = null!;
        public string Correu { get; set; } = null!;
        public string? Idioma { get; set; }
    }
}
