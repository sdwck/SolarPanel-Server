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
            {
                _context.ModeResults.Update(modeResult);
                await _context.SaveChangesAsync();
            }
        }

        return modeResult ?? await _context.ModeResults.OrderByDescending(x => x.Id).FirstAsync();
    }

    public async Task<ModeResult> SaveModeResultAsync(ModeResult modeResult)
    {
        var existingModeResult = await _context.ModeResults.FirstOrDefaultAsync();
        if (existingModeResult == null)
        {
            _context.ModeResults.Add(modeResult);
        }
        else
        {
            existingModeResult.BatteryMode = modeResult.BatteryMode;
            existingModeResult.LoadMode = modeResult.LoadMode;
            _context.ModeResults.Update(existingModeResult);
        }

        await _context.SaveChangesAsync();
        return existingModeResult ?? modeResult;
    }
}