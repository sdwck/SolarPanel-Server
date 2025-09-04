using Microsoft.EntityFrameworkCore;
using SolarPanel.Core.Entities;
using SolarPanel.Core.Interfaces;
using SolarPanel.Infrastructure.Data;

namespace SolarPanel.Infrastructure.Repositories;

public class SolarDataRepository : ISolarDataRepository
{
    private readonly SolarPanelDbContext _context;

    public SolarDataRepository(SolarPanelDbContext context)
    {
        _context = context;
    }

    public async Task<SolarData> AddAsync(SolarData solarData)
    {
        var entry = await _context.SolarData.AddAsync(solarData);
        await _context.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task<SolarData?> GetByIdAsync(int id)
    {
        return await _context.SolarData
            .Include(x => x.BatteryData)
            .Include(x => x.PowerData)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<SolarData?> GetLatestAsync()
    {
        return await _context.SolarData
            .Include(x => x.BatteryData)
            .Include(x => x.PowerData)
            .OrderByDescending(x => x.Timestamp)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<SolarData>> GetAllAsync(int page = 1, int pageSize = 50)
    {
        return await _context.SolarData
            .Include(x => x.BatteryData)
            .Include(x => x.PowerData)
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<SolarData>> GetByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _context.SolarData
            .Include(x => x.BatteryData)
            .Include(x => x.PowerData)
            .Where(x => x.Timestamp >= from && x.Timestamp <= to)
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync();
    }

    public async Task<BatteryData?> GetBatteryDataAsync(int solarDataId)
    {
        return await _context.BatteryData
            .FirstOrDefaultAsync(x => x.SolarDataId == solarDataId);
    }

    public async Task<PowerData?> GetPowerDataAsync(int solarDataId)
    {
        return await _context.PowerData
            .FirstOrDefaultAsync(x => x.SolarDataId == solarDataId);
    }

    public Task<int> GetTotalCountAsync()
    {
        return _context.SolarData.CountAsync();
    }
}