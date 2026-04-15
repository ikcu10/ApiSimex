namespace ApiSimex.ViewModels
{
    public class NotificacionOfertaDTO
    {
        public int Id { get; set; }
        public string NombreOperador { get; set; } = string.Empty;
        public DateOnly? FechaInicio { get; set; }
        public DateOnly? FechaCaducidad { get; set; }
        public decimal? Precio { get; set; }
    }
}
