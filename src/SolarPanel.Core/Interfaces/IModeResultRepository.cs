using SolarPanel.Core.Entities;

namespace SolarPanel.Core.Interfaces;

public interface IModeResultRepository
{
    Task<ModeResult> GetModeResultAsync();
    Task SaveModeResultAsync(ModeResult modeResult);
}