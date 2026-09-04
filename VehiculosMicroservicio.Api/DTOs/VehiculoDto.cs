namespace VehiculosMicroservicio.Api.DTOs
{
    public class CrearVehiculoDto
    {
        public int IdCategoria { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public bool Estado { get; set; } = true;
    }

    public class ActualizarVehiculoDto
    {
        public int IdCategoria { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public bool Estado { get; set; }
    }
}
