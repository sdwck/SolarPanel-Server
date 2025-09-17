using SolarPanel.Application.DTOs;

namespace SolarPanel.Application.Interfaces;

public interface IHistoryService
{
    Task<List<HistoryDataDto>> GetHistoryDataAsync(string timeRange, DateTime? from = null, DateTime? to = null);
}