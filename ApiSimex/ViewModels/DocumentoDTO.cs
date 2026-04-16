namespace ApiSimex.ViewModels
{
    public class DocumentoDTO
    {
        public int IdDocumento { get; set; }
        public string NombreDocumento { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
    }
}
