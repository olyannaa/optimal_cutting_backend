using Microsoft.EntityFrameworkCore;
using vega.Migrations.DAL;
using System.ComponentModel;
using System.Data;
using System.Numerics;

namespace vega.Migrations.EF
{
    public class VegaContext : DbContext
    {
        
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Figure> Figures { get; set; } = null!;
        public virtual DbSet<Material> Materials { get; set; } = null!;
        public virtual DbSet<GeometryType> GeometryTypes { get; set; } = null!;
        public virtual DbSet<Filename> Filenames { get; set; } = null!;


        public VegaContext(DbContextOptions<VegaContext> options)
            : base(options)
        {

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(Console.WriteLine);
            optionsBuilder.EnableSensitiveDataLogging();
        }
    }
}
