namespace ApiSimex.ViewModels
{
    public class PedidoAgenteDTO
    {
        public int Id { get; set; }
        public string PedidoCodigo { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public string PasoActualNombre { get; set; } = string.Empty;
    }
}
