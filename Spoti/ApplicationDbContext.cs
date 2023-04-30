using Microsoft.EntityFrameworkCore;
using Spoti;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Album> Albums { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=LAPTOP-SMD5MSK3;Database=SPOTI;Trusted_Connection=True;TrustServerCertificate=true");
    }
}
