using Microsoft.EntityFrameworkCore;
using SolarPanel.Core.Entities;
using SolarPanel.Core.Interfaces;
using SolarPanel.Infrastructure.Data;

namespace SolarPanel.Infrastructure.Repositories;

public class ModeResultRepository : IModeResultRepository
{
    private readonly AppDbContext _context;

    public ModeResultRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ModeResult> GetModeResultAsync()
    {
        var modeResult = await _context.ModeResults.FirstOrDefaultAsync();

        if (modeResult is null)
        {
            _context.ModeResults.Add(new ModeResult { BatteryMode = "PCP00", LoadMode = "POP00" });
            await _context.SaveChangesAsync();
        }
        else
        {
            var changesMade = false;
            if (string.IsNullOrWhiteSpace(modeResult.BatteryMode))
            {
                modeResult.BatteryMode = "PCP00";
                changesMade = true;
            }

            if (string.IsNullOrWhiteSpace(modeResult.LoadMode))
            {
                modeResult.LoadMode = "POP00";
                changesMade = true;
            }

            if (changesMade)
                await _context.SaveChangesAsync();
        }

        return modeResult ?? await _context.ModeResults.FirstAsync();
    }

    public async Task SaveModeResultAsync(ModeResult modeResult)
    {
        var existingModeResult = await _context.ModeResults.FirstOrDefaultAsync();
        if (existingModeResult is null)
        {
            _context.ModeResults.Add(modeResult);
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(modeResult.BatteryMode))
                existingModeResult.BatteryMode = modeResult.BatteryMode;
            if (!string.IsNullOrWhiteSpace(modeResult.LoadMode))
                existingModeResult.LoadMode = modeResult.LoadMode;
        }

        await _context.SaveChangesAsync();
    }
}