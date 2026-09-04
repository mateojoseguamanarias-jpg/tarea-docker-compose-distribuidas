using Microsoft.EntityFrameworkCore;
using VehiculosMicroservicio.Api.Models;

namespace VehiculosMicroservicio.Api.Data
{
    public class VehiculosDbContext : DbContext
    {
        public VehiculosDbContext(DbContextOptions<VehiculosDbContext> options) : base(options)
        {
        }

        public DbSet<Vehiculo> Vehiculos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Vehiculo>(entity =>
            {
                entity.HasKey(e => e.IdVehiculo);
                entity.Property(e => e.Marca).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Modelo).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Precio).HasColumnType("decimal(12,2)");
                entity.Property(e => e.Stock).HasDefaultValue(0);
                entity.Property(e => e.Estado).HasDefaultValue(true);
            });
        }
    }
}
