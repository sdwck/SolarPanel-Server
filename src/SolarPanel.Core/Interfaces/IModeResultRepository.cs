using SolarPanel.Core.Entities;

namespace SolarPanel.Core.Interfaces;

public interface IModeResultRepository
{
    Task<ModeResult> GetModeResultAsync();
    Task<ModeResult> SaveModeResultAsync(ModeResult modeResult);
}