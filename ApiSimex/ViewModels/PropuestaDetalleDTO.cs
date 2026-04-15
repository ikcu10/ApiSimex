namespace ApiSimex.ViewModels
{
    public class PropuestaDetalleDTO
    {
        public int Id { get; set; }
        public string OperadorLogistico { get; set; } = string.Empty;
        public DateOnly? FechaCaducidad { get; set; }
        public string Origen { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public string TipoTransporte { get; set; } = string.Empty;
        public string Incoterm { get; set; } = string.Empty;
        public double? Peso { get; set; }
        public double? Volumen { get; set; }
        public string Flujo { get; set; } = string.Empty;
        public decimal? Precio { get; set; }
    }
}
