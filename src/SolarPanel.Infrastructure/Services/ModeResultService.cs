using System.Threading.Tasks;
using SolarPanel.Application.DTOs;
using SolarPanel.Application.Interfaces;
using SolarPanel.Core.Entities;
using SolarPanel.Core.Interfaces;

namespace SolarPanel.Infrastructure.Services;

public class ModeResultService : IModeResultService
{
    private readonly IModeResultRepository _repository;
    public ModeResultService(IModeResultRepository repository)
    {
        _repository = repository;
    }

    public async Task SaveModeResultAsync(ModeResultDto dto)
    {
        var entity = new ModeResult
        {
            BatteryMode = dto.BatteryMode,
            LoadMode = dto.LoadMode
        };
        await _repository.SaveModeResultAsync(entity);
    }

    public async Task<ModeResultDto?> GetCurrentModeResultAsync()
    {
        var entity = await _repository.GetModeResultAsync();
        if (entity == null) return null;
        return new ModeResultDto
        {
            BatteryMode = entity.BatteryMode,
            LoadMode = entity.LoadMode
        };
    }
}

