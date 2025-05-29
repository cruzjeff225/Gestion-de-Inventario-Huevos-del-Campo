using Gestión_de_Inventario_Huevos_del_Campo.Models;
using Microsoft.EntityFrameworkCore;

namespace Gestión_de_Inventario_Huevos_del_Campo.Db
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Productos> Productos { get; set; }

        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Productos>()
                .Property(p => p.PrecioVenta)
                .HasPrecision(18, 2); // 18 dígitos en total, 2 después del punto decimal

            base.OnModelCreating(modelBuilder);
        }

    }
}
