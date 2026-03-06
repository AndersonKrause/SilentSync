using Microsoft.EntityFrameworkCore;
using SilentSync.Api.Models;

namespace SilentSync.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<RoomMember> RoomMembers => Set<RoomMember>();

    public DbSet<Room> Rooms => Set<Room>();
    
    public DbSet<AppUser> Users => Set<AppUser>();
    
    public DbSet<LoginCode> LoginCodes => Set<LoginCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Room>()
            .HasIndex(r => r.Code)
            .IsUnique();

        modelBuilder.Entity<Room>()
            .Property(r => r.Code)
            .HasMaxLength(10)
            .IsRequired();
        
        modelBuilder.Entity<RoomMember>(entity =>
        {
            entity.Property(x => x.DisplayName).HasMaxLength(80);
            entity.Property(x => x.DeviceId).HasMaxLength(200);

            // The same device cannot connect twice in the same room.
            entity.HasIndex(x => new { x.RoomId, x.DeviceId }).IsUnique();
            
            entity.HasIndex(x => new { x.RoomId, x.UserId }).IsUnique();
        });
        
        modelBuilder.Entity<AppUser>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<AppUser>()
            .Property(x => x.Email)
            .HasMaxLength(200);
        
        modelBuilder.Entity<AppUser>()
            .Property(x => x.PasswordHash)
            .HasMaxLength(400);
        
        modelBuilder.Entity<AppUser>()
            .Property(x => x.PendingPasswordHash)
            .HasMaxLength(400);

        modelBuilder.Entity<LoginCode>()
            .HasIndex(x => new { x.Email, x.Code })
            .IsUnique();

        modelBuilder.Entity<LoginCode>()
            .Property(x => x.Email)
            .HasMaxLength(200);

        modelBuilder.Entity<LoginCode>()
            .Property(x => x.Code)
            .HasMaxLength(10);
    }
}
