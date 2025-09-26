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
        var avgInput = data.Where(d => d.PowerData != null).Average(d => d.PowerData!.PvInputPower);
        var converted = data.Where(d => d is { PowerData: not null, BatteryData: not null })
            .Select(x => MapToHistoryDto(x, avgInput))
            .ToList();
        var filtered = new List<HistoryDataDto>();
        foreach (var solarData in converted.Where(solarData => !filtered.Any(x =>
                     x.Timestamp.Date == solarData.Timestamp.Date &&
                     x.Timestamp.Hour == solarData.Timestamp.Hour &&
                     x.Timestamp.Minute == solarData.Timestamp.Minute)))
            filtered.Add(solarData);

        return filtered;
    }

    private static HistoryDataDto MapToHistoryDto(SolarData data, double avgInput)
    {
        var lowerBound = avgInput * 0.5;
        var upperBound = avgInput * 2;

        string status;
        
        if (data.PowerData!.PvInputPower >= upperBound)
            status = "optimal";
        else if (data.PowerData.PvInputPower >= lowerBound)
            status = "good";
        else if (data.PowerData.PvInputPower == 0)
            status = "none";
        else
            status = "low";

        return new HistoryDataDto
        {
            Timestamp = data.Timestamp,
            SolarInput = data.PowerData!.PvInputPower,
            BatteryLevel = data.BatteryData!.BatteryCapacity,
            PowerOutput = data.PowerData.AcOutputActivePower,
            Temperature = data.InverterHeatSinkTemperature,
            Status = status
        };
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