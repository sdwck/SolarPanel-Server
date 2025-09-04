using Microsoft.EntityFrameworkCore;
using SolarPanel.Core.Entities;

namespace SolarPanel.Infrastructure.Data;

public class SolarPanelDbContext : DbContext
{
    public SolarPanelDbContext(DbContextOptions<SolarPanelDbContext> options) : base(options)
    {
    }
    
    public DbSet<SolarData> SolarData { get; set; }
    public DbSet<BatteryData> BatteryData { get; set; }
    public DbSet<PowerData> PowerData { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SolarData>(entity =>
        {
            entity.HasIndex(e => e.Timestamp);
            entity.HasOne(e => e.BatteryData).WithOne(e => e.SolarData)
                .HasForeignKey<BatteryData>(e => e.SolarDataId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.PowerData).WithOne(e => e.SolarData)
                .HasForeignKey<PowerData>(e => e.SolarDataId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}