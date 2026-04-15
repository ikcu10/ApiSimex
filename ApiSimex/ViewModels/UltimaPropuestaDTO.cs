namespace ApiSimex.ViewModels
{
    public class UltimaPropuestaDTO
    {
        public string NombreOperador { get; set; } = string.Empty;
        public decimal? Precio { get; set; }
        public DateOnly? FechaInicio { get; set; }
        public DateOnly? FechaCaducidad { get; set; }
    }
}
