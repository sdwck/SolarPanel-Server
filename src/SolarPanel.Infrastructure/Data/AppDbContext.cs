using Microsoft.EntityFrameworkCore;
using SolarPanel.Core.Entities;

namespace SolarPanel.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<MaintenanceTask> MaintenanceTasks { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

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
        
        base.OnModelCreating(modelBuilder);
    }
}