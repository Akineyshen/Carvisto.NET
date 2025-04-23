using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Carvisto.Models;

namespace Carvisto.Data
{
    public class CarvistoDbContext : IdentityDbContext<ApplicationUser>
    {
        public CarvistoDbContext(DbContextOptions<CarvistoDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Trip> Trips { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasKey(u => u.Id);
            
            modelBuilder.Entity<Trip>()
                .HasOne(t => t.Driver)
                .WithMany()
                .HasForeignKey(t => t.DriverId)
                .IsRequired();
        }
    }
}