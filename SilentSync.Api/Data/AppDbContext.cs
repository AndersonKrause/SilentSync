using Microsoft.EntityFrameworkCore;
using SilentSync.Api.Models;

namespace SilentSync.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<RoomMember> RoomMembers => Set<RoomMember>();

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
        
        modelBuilder.Entity<RoomMember>(entity =>
        {
            entity.Property(x => x.DisplayName).HasMaxLength(80);
            entity.Property(x => x.DeviceId).HasMaxLength(200);

            // The same device cannot connect twice in the same room.
            entity.HasIndex(x => new { x.RoomId, x.DeviceId }).IsUnique();
        });
    }
}
