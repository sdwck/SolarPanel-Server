using SolarPanel.Application.DTOs;
using SolarPanel.Application.Interfaces;
using SolarPanel.Core.Entities;
using SolarPanel.Core.Interfaces;

namespace SolarPanel.Infrastructure.Services;

public class HistoryService : IHistoryService
{
    private readonly ISolarDataRepository _repository;

    public HistoryService(ISolarDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<HistoryDataDto>> GetHistoryDataAsync(string timeRange, DateTime? from = null,
        DateTime? to = null)
    {
        DateTime startDate, endDate;
        int gap;

        if (from.HasValue && to.HasValue)
        {
            startDate = from.Value;
            endDate = to.Value;
            gap = 60;
        }
        else
        {
            (startDate, endDate, gap) = GetDateRange(timeRange);
        }

        var data = (await _repository.GetByDateRangeAsync(startDate, endDate, gap)).ToList();
        var filtered = data.Where(d => d is { PowerData: not null, BatteryData: not null })
            .OrderBy(d => d.Timestamp)
            .ToList();

        var result = new List<HistoryDataDto>();
        for (int i = 0; i < filtered.Count - 1; i++)
        {
            var current = filtered[i];
            var next = filtered[i + 1];
            var deltaHours = (next.Timestamp - current.Timestamp).TotalMinutes / 60.0;
            if (deltaHours <= 0) continue;
            
            var avgPvInput = (current.PowerData.PvInputPower + next.PowerData.PvInputPower) / 2.0;
            var avgAcOutput = (current.PowerData.AcOutputActivePower + next.PowerData.AcOutputActivePower) / 2.0;
            var pvEnergy = avgPvInput * deltaHours;
            var acEnergy = avgAcOutput * deltaHours;
            double efficiency = (pvEnergy >= 0.01 && acEnergy > 0) ? (pvEnergy / acEnergy) * 100 : 0;
            var status = efficiency switch
            {
                >= 85 => "optimal",
                >= 70 => "good",
                _ => "low"
            };
            result.Add(new HistoryDataDto
            {
                Timestamp = current.Timestamp,
                SolarInput = avgPvInput, 
                BatteryLevel = current.BatteryData.BatteryCapacity,
                PowerOutput = avgAcOutput, 
                Temperature = current.InverterHeatSinkTemperature,
                Efficiency = efficiency,
                Status = status
            });
        }
        
        if (filtered.Count == 1)
        {
            var current = filtered[0];
            var pvEnergy = current.PowerData.PvInputPower * (gap / 60.0);
            var acEnergy = current.PowerData.AcOutputActivePower * (gap / 60.0);
            double efficiency = (pvEnergy >= 0.01 && acEnergy > 0) ? (pvEnergy / acEnergy) * 100 : 0;
            var status = efficiency switch
            {
                >= 85 => "optimal",
                >= 70 => "good",
                _ => "low"
            };
            result.Add(new HistoryDataDto
            {
                Timestamp = current.Timestamp,
                SolarInput = current.PowerData.PvInputPower,
                BatteryLevel = current.BatteryData.BatteryCapacity,
                PowerOutput = current.PowerData.AcOutputActivePower,
                Temperature = current.InverterHeatSinkTemperature,
                Efficiency = efficiency,
                Status = status
            });
        }
        return result;
    }

    private static (DateTime from, DateTime to, int gap) GetDateRange(string timeRange)
    {
        var now = DateTime.UtcNow;

        var minutes = now.Minute;

        var startOfInterval = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var endOfInterval = new DateTime(now.Year, now.Month, now.Day, now.Hour, minutes > 30 ? 30 : 0, 59,
            DateTimeKind.Utc);

        return timeRange.ToLower() switch
        {
            "today" => (startOfInterval, endOfInterval, 30),
            "3days" => (startOfInterval.AddDays(-3), endOfInterval, 30),
            "week" => (startOfInterval.AddDays(-7), endOfInterval, 60),
            "month" => (startOfInterval.AddDays(-30), endOfInterval, 60),
            _ => (startOfInterval, endOfInterval, 0)
        };
    }
}
