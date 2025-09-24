using System.Threading.Tasks;
using SolarPanel.Application.DTOs;

namespace SolarPanel.Application.Interfaces;

public interface IModeResultService
{
    Task SaveModeResultAsync(ModeResultDto dto);
    Task<ModeResultDto?> GetCurrentModeResultAsync();
}

