using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VehiculosMicroservicio.Api.Models
{
    [Table("Vehiculos")]
    public class Vehiculo
    {
        [Key]
        [Column("IdVehiculo")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdVehiculo { get; set; }

        [Required]
        [Column("IdCategoria")]
        public int IdCategoria { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Marca")]
        public string Marca { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("Modelo")]
        public string Modelo { get; set; } = string.Empty;

        [Column("Precio", TypeName = "decimal(12,2)")]
        public decimal Precio { get; set; }

        [Column("Stock")]
        public int Stock { get; set; }

        [Column("Estado")]
        public bool Estado { get; set; } = true;
    }
}
