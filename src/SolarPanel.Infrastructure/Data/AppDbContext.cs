using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SolarPanel.Core.Entities;

namespace SolarPanel.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<MaintenanceTask> MaintenanceTasks { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ModeResult> ModeResults { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MaintenanceTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Roles)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );
        });
        
        modelBuilder.Entity<ModeResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BatteryMode).IsRequired();
            entity.Property(e => e.LoadMode).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}