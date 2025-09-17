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

    public async Task<IEnumerable<SolarData>> GetByDateRangeAsync(DateTime from, DateTime to, int? gapInRecords = null, int? count = null)
    {
        if (from > to) throw new ArgumentException("'from' must be earlier than or equal to 'to'", nameof(from));
        if (count.HasValue) return await GetSampledByCountAsync(from, to, count.Value);

        var query = _context.SolarData
            .Include(x => x.BatteryData)
            .Include(x => x.PowerData)
            .Where(x => x.Timestamp >= from && x.Timestamp <= to);

        if (gapInRecords.HasValue)
        {
            var gapHours = gapInRecords.Value / 60;
            var gapMinutes = gapInRecords.Value % 60;
            if (gapHours != 0) query = query.Where(x => x.Timestamp.Hour % gapHours == 0);
            if (gapMinutes != 0) query = query.Where(x => x.Timestamp.Minute % gapMinutes == 0);
            else if (gapHours != 0) query = query.Where(x => x.Timestamp.Minute == 0);
        }

        var result = await query
            .OrderByDescending(x => x.Timestamp)
            .ToListAsync();
        
        foreach (var item in result)
        {
            item.Timestamp = DateTime.SpecifyKind(item.Timestamp, DateTimeKind.Utc);
        }

        if (gapInRecords is not < 60) return result;
        var seen = new HashSet<(int hour, int minute)>();
        var filtered = new List<SolarData>(result.Count);
        foreach (var item in result)
        {
            var key = (item.Timestamp.Hour, item.Timestamp.Minute);
            if (seen.Add(key))
            {
                filtered.Add(item);
            }
        }
        return filtered;
    }

    private async Task<IEnumerable<SolarData>> GetSampledByCountAsync(DateTime from, DateTime to, int count)
    {
        if (count <= 0) return [];

        var idTsList = await _context.SolarData
            .AsNoTracking()
            .Where(x => x.Timestamp >= from && x.Timestamp <= to)
            .OrderBy(x => x.Timestamp)
            .Select(x => new { x.Id, x.Timestamp })
            .ToListAsync();

        int total = idTsList.Count;
        if (total == 0) return [];
        if (count >= total)
        {
            return await _context.SolarData
                .AsNoTracking()
                .Include(x => x.BatteryData)
                .Include(x => x.PowerData)
                .Where(x => x.Timestamp >= from && x.Timestamp <= to)
                .OrderByDescending(x => x.Timestamp)
                .ToListAsync();
        }

        var targets = new List<DateTime>(count);
        if (count == 1) targets.Add(to);
        else
        {
            long rangeTicks = (to - from).Ticks;
            for (int k = 0; k < count; k++)
            {
                long offset = (long)Math.Round((double)k * rangeTicks / (count - 1));
                targets.Add(from.AddTicks(offset));
            }
        }

        var used = new bool[total];
        var selectedIndices = new List<int>(count);
        int pos = 0;

        foreach (var target in targets)
        {
            while (pos < total && idTsList[pos].Timestamp < target) pos++;

            int best;
            if (pos == 0) best = 0;
            else if (pos == total) best = total - 1;
            else
            {
                var prev = idTsList[pos - 1].Timestamp;
                var curr = idTsList[pos].Timestamp;
                best = (Math.Abs((curr - target).Ticks) < Math.Abs((target - prev).Ticks)) ? pos : pos - 1;
            }

            if (used[best])
            {
                int offset = 1;
                while (true)
                {
                    int lower = best - offset;
                    int upper = best + offset;
                    int choose = -1;
                    if (lower >= 0 && !used[lower]) choose = lower;
                    else if (upper < total && !used[upper]) choose = upper;
                    if (choose >= 0)
                    {
                        best = choose;
                        break;
                    }
                    offset++;
                }
            }

            used[best] = true;
            selectedIndices.Add(best);
        }

        var selectedIds = selectedIndices.Select(i => idTsList[i].Id).ToList();

        var records = await _context.SolarData
            .AsNoTracking()
            .Include(x => x.BatteryData)
            .Include(x => x.PowerData)
            .Where(x => selectedIds.Contains(x.Id))
            .ToListAsync();

        return records.OrderByDescending(x => x.Timestamp).ToList();
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
