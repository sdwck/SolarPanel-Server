using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SolarPanel.Core.Entities;

namespace SolarPanel.Infrastructure.Data;

public class SolarPanelDbContext : DbContext
{
    private readonly IHostEnvironment _environment;

    private const string NonProductionSaveChangesErrorMessage =
        "SaveChanges for SolarPanelDbContext is disabled in non-production environments.";

    public SolarPanelDbContext(DbContextOptions<SolarPanelDbContext> options, IHostEnvironment environment)
        : base(options)
    {
        _environment = environment;
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

    public override int SaveChanges()
    {
        if (_environment.IsDevelopment() && ChangeTracker.HasChanges())
            throw new InvalidOperationException(NonProductionSaveChangesErrorMessage);
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        if (_environment.IsDevelopment() && ChangeTracker.HasChanges())
            throw new InvalidOperationException(NonProductionSaveChangesErrorMessage);
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_environment.IsDevelopment() && ChangeTracker.HasChanges())
            throw new InvalidOperationException(NonProductionSaveChangesErrorMessage);
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        if (_environment.IsDevelopment() && ChangeTracker.HasChanges())
            throw new InvalidOperationException(NonProductionSaveChangesErrorMessage);
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}