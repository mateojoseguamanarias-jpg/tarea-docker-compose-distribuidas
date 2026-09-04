using CategoriasMicroservicio.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CategoriasMicroservicio.Api.Data
{
    public class CategoriasDbContext : DbContext
    {
        public CategoriasDbContext(DbContextOptions<CategoriasDbContext> options) : base(options)
        {
        }

        public DbSet<Categoria> Categorias { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasKey(e => e.IdCategoria);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcion).HasMaxLength(250);
                entity.Property(e => e.Estado).HasDefaultValue(true);
            });
        }
    }
}
