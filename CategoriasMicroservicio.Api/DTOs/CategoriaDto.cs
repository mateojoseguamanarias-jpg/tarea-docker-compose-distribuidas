namespace CategoriasMicroservicio.Api.DTOs
{
    public class CrearCategoriaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Estado { get; set; } = true;
    }

    public class ActualizarCategoriaDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Estado { get; set; }
    }
}
