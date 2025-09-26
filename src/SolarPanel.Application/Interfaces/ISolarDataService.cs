using SolarPanel.Application.DTOs;

namespace SolarPanel.Application.Interfaces;

public interface ISolarDataService
{
    Task<SolarDataDto> SaveSolarDataAsync(SolarPanelDataJsonDto reading);
    Task<SolarDataDto?> GetLatestDataAsync();
    Task<SolarDataDto?> GetByIdAsync(int id);
    Task<PaginatedResponse<SolarDataDto>> GetAllAsync(int page = 1, int pageSize = 50);

    Task<IEnumerable<SolarDataDto>> GetByDateRangeAsync(DateTime from, DateTime to, int? gapInRecords = null,
        int? count = null);
    Task<EnergyResponseDto> GetEnergyProducedAsync(DateTime from, DateTime to, string source, int? gapInRecords = null);
}