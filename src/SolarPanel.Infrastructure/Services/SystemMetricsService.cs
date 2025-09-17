using SolarPanel.Application.DTOs;
using SolarPanel.Application.Interfaces;
using SolarPanel.Core.Interfaces;

namespace SolarPanel.Infrastructure.Services;

public class SystemMetricsService : ISystemMetricsService
{
    private readonly ISolarDataRepository _repository;

    public SystemMetricsService(ISolarDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<SystemMetricsDto> GetSystemMetricsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var todayData = await _repository.GetByDateRangeAsync(today, DateTime.UtcNow, 5);
        var dataList = todayData.Where(d => d.PowerData != null).ToList();

        if (dataList.Count == 0)
        {
            return new SystemMetricsDto
            {
                TotalPanels = 1,
                ActivePanels = 0,
                TotalPowerGenerated = 0,
                AverageEfficiency = 0,
                TotalEnergyToday = 0,
                SystemUptime = 0
            };
        }

        const int totalPanels = 1;
        var activePanels = dataList.Any(d => d.PowerData!.PvInputPower > 0) ? 1 : 0;
        var totalPower = dataList.Max(d => d.PowerData!.PvInputPower); // TODO: Replace with real total power calculation
        var avgEfficiency = 90.0;
        var totalEnergyToday = dataList
            .GroupBy(d => d.Timestamp.Date)
            .Average(g => g.Sum(d => (double)d.PowerData!.PvInputPower) * 60d / dataList.Count(x => x.Timestamp.Date == g.Key) / 1000.0);
        var uptime = (double)dataList.Count(d => d.IsSwitchedOn) / dataList.Count * 100;

        return new SystemMetricsDto
        {
            TotalPanels = totalPanels,
            ActivePanels = activePanels,
            TotalPowerGenerated = totalPower,
            AverageEfficiency = Math.Round(avgEfficiency, 1),
            TotalEnergyToday = Math.Round(totalEnergyToday, 2),
            SystemUptime = Math.Round(uptime, 1)
        };
    }
}