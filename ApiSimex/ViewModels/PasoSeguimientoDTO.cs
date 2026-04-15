namespace ApiSimex.ViewModels
{
    public class PasoSeguimientoDTO
    {
        public int TrackingStepId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty; // "Pendent", "En curs", "Completat"
        public int Orden { get; set; }
    }
}
