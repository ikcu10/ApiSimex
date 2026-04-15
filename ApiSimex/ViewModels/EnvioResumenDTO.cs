namespace ApiSimex.ViewModels
{
    public class EnvioResumenDTO
    {
        public int Id { get; set; }
        public string CiudadOrigen { get; set; } = string.Empty;
        public string CiudadDestino { get; set; } = string.Empty;
        public string PasoActual { get; set; } = string.Empty;
    }
}
