using backendnet.Data.Seed;
using backendnet.Models;
using Microsoft.EntityFrameworkCore;

namespace backendnet.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
    {
        public DbSet<Pelicula> Pelicula { get; set; }
        public DbSet<Categoria> Categoria { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new SeedCategoria());
            modelBuilder.ApplyConfiguration(new SeedPelicula());
        }
    }
}