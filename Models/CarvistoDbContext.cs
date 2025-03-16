using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Carvisto.Models
{
    // Контекст базы данных, который будет связывать модель с базой SQLite
    public class CarvistoDbContext : IdentityDbContext
    {
        public CarvistoDbContext(DbContextOptions<CarvistoDbContext> options)
            : base(options)
        {
        }
        
        // DbSet для хранения данных о путешествиях
        public DbSet<Trip> Trips { get; set; }
    }
}