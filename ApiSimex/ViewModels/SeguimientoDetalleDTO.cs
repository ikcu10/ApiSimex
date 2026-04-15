namespace ApiSimex.ViewModels
{
    public class SeguimientoDetalleDTO
    {
        public int Id { get; set; }
        public string PedidoCodigo { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;

        // Esta es la lista que dibujará el Timeline en Android
        public List<PasoSeguimientoDTO> Pasos { get; set; } = new List<PasoSeguimientoDTO>();
    }
}
