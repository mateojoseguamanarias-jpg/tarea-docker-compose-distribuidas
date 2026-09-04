using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CategoriasMicroservicio.Api.Models
{
    [Table("Categorias")]
    public class Categoria
    {
        [Key]
        [Column("IdCategoria")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdCategoria { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(250)]
        [Column("Descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [Column("Estado")]
        public bool Estado { get; set; } = true;
    }
}
