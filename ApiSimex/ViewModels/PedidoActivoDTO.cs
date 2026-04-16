namespace ApiSimex.ViewModels
{
    public class PedidoActivoDTO
    {
        public int IdPedido { get; set; }
        public string PasoActual { get; set; } = string.Empty;
        public string CiudadOrigen { get; set; } = string.Empty;
        public string CiudadDestino { get; set; } = string.Empty;
    }
}
