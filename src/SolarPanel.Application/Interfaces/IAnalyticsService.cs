using SolarPanel.Application.DTOs;

namespace SolarPanel.Application.Interfaces;

public interface IAnalyticsService
{
    Task<AnalyticsDataDto> GetAnalyticsDataAsync(string timeRange);
}