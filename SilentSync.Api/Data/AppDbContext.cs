using Microsoft.EntityFrameworkCore;
using SilentSync.Api.Models;

namespace SilentSync.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Room> Rooms => Set<Room>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Room>()
            .HasIndex(r => r.Code)
            .IsUnique();

        modelBuilder.Entity<Room>()
            .Property(r => r.Code)
            .HasMaxLength(10);
    }
}
