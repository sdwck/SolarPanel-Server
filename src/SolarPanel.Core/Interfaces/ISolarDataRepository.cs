using SolarPanel.Core.Entities;

namespace SolarPanel.Core.Interfaces;

public interface ISolarDataRepository
{
    Task<SolarData> AddAsync(SolarData solarData);
    Task<SolarData?> GetByIdAsync(int id);
    Task<SolarData?> GetLatestAsync();
    Task<IEnumerable<SolarData>> GetAllAsync(int page = 1, int pageSize = 50);
    Task<IEnumerable<SolarData>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<BatteryData?> GetBatteryDataAsync(int solarDataId);
    Task<PowerData?> GetPowerDataAsync(int solarDataId);
    Task<int> GetTotalCountAsync();
}