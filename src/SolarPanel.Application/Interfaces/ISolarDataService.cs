using SolarPanel.Application.DTOs;

namespace SolarPanel.Application.Interfaces;

public interface ISolarDataService
{
    Task<SolarDataDto> SaveSolarDataAsync(SolarPanelDataJsonDto reading);
    Task<SolarDataDto?> GetLatestDataAsync();
    Task<SolarDataDto?> GetByIdAsync(int id);
    Task<SolarDataResponseDto> GetAllAsync(int page = 1, int pageSize = 50);
    Task<IEnumerable<SolarDataDto>> GetByDateRangeAsync(DateTime from, DateTime to);
}