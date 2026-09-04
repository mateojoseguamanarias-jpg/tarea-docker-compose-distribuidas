namespace VehiculosMicroservicio.Api.Events
{
    public class CategoriaCreadaEvento
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Estado { get; set; }
        public DateTime? FechaEvento { get; set; }
    }
}
