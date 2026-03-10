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

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasIndex(r => r.Code).IsUnique();

            entity.Property(r => r.Code)
                .HasMaxLength(10)
                .IsRequired();
            
            entity.HasOne(r => r.Owner)
                .WithMany()
                .HasForeignKey(r => r.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RoomMember>(entity =>
        {
            entity.Property(x => x.DisplayName)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(x => x.DeviceId)
                .HasMaxLength(200)
                .IsRequired();

            entity.HasOne(x => x.Room)
                .WithMany(r => r.Members)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.User)
                .WithMany(u => u.RoomMembers)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(x => new { x.RoomId, x.DeviceId }).IsUnique();
            entity.HasIndex(x => new { x.RoomId, x.UserId });
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();

            entity.Property(x => x.Email)
                .HasMaxLength(200)
                .IsRequired();
            
            entity.Property(x => x.Role)
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("user");

            entity.Property(x => x.PasswordHash)
                .HasMaxLength(400);

            entity.Property(x => x.PendingPasswordHash)
                .HasMaxLength(400);
        });

        modelBuilder.Entity<LoginCode>(entity =>
        {
            entity.HasIndex(x => new { x.Email, x.Code }).IsUnique();

            entity.Property(x => x.Email)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Code)
                .HasMaxLength(10)
                .IsRequired();
        });
    }
}
