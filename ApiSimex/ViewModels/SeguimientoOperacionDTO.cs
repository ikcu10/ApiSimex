namespace ApiSimex.ViewModels
{
    public class SeguimientoOperacionDTO
    {
        public int IdOperacion { get; set; }
        public string CiudadOrigen { get; set; } = string.Empty;
        public string CiudadDestino { get; set; } = string.Empty;
        public string Incoterm { get; set; } = string.Empty;

        public List<PasoTrackingDTO> Tracking { get; set; } = new List<PasoTrackingDTO>();
    }
}
