namespace ApiSimex.ViewModels
{
    public class HistorialPedidoDTO
    {
        public int IdPedido { get; set; }
        public string CiudadOrigen { get; set; } = string.Empty;
        public string CiudadDestino { get; set; } = string.Empty;
        public DateOnly? FechaFinalizacion { get; set; }
    }
}
