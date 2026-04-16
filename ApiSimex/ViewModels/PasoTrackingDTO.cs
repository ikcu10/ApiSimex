namespace ApiSimex.ViewModels
{
    public class PasoTrackingDTO
    {
        public string NombrePaso { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime? FechaCompletado { get; set; }
        public int Orden { get; set; }
    }
}
