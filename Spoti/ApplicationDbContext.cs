using Microsoft.EntityFrameworkCore;
using Spoti;

public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Album> Albums { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=DESKTOP-9HL0FUT;Database=test;Trusted_Connection=True;TrustServerCertificate=true");
    }
}
