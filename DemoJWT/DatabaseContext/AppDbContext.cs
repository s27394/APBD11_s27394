using DemoJWT.Model;
using Microsoft.EntityFrameworkCore;

namespace DemoJWT.DatabaseContext

{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {

        }

        public AppDbContext(DbContextOptions options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>().ToTable("Users");

            modelBuilder.Entity<AppUser>()
                .Property(u => u.RefreshToken)
                .HasDefaultValue(null);

            modelBuilder.Entity<AppUser>()
                .Property(u => u.RefreshTokenExp)
                .HasDefaultValue(null);
        }
        public DbSet<AppUser> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=master;User Id=SA;Password=yourStrong(!)Password;TrustServerCertificate=true");
        }

    }
}
