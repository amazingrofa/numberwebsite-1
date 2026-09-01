using Microsoft.EntityFrameworkCore;

namespace WebApplication2.Data
{
    public class NumberPage
    {
        public long Id { get; set; }
        public long ViewCount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<NumberPage> NumberPages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NumberPage>()
                .HasIndex(n => n.ViewCount);
        }
    }
}