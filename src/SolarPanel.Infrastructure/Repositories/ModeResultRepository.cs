using Microsoft.EntityFrameworkCore;
using SolarPanel.Core.Entities;
using SolarPanel.Core.Interfaces;
using SolarPanel.Infrastructure.Data;

namespace SolarPanel.Infrastructure.Repositories;

public class ModeResultRepository: IModeResultRepository
{
    private readonly AppDbContext _context;
    public ModeResultRepository(AppDbContext context)
    {
        _context = context;
    }
        
    public async Task<ModeResult> GetModeResultAsync()
    {
        
        return await _context.ModeResults.OrderByDescending(x => x.Id).FirstOrDefaultAsync();
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