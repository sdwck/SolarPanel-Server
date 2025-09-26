using SolarPanel.Application.DTOs;
using SolarPanel.Application.Interfaces;
using SolarPanel.Core.Interfaces;

namespace SolarPanel.Infrastructure.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ISolarDataRepository _repository;
    private readonly ISolarDataService _solarDataService;

    public AnalyticsService(ISolarDataRepository repository, ISolarDataService solarDataService)
    {
        _repository = repository;
        _solarDataService = solarDataService;
    }

    public async Task<AnalyticsDataDto> GetAnalyticsDataAsync(string timeRange)
    {
        var (from, to, gap) = GetDateRange(timeRange);

        var lastMonthFrom = from.AddMonths(-1);
        var lastMonthTo = to.AddMonths(-1);


        var totalProduced = (await _solarDataService.GetEnergyProducedAsync(from, to, "pv")).EnergyKWh;
        var totalConsumed = (await _solarDataService.GetEnergyProducedAsync(from, to, "ac")).EnergyKWh;
        var lastMonthProd = (await _solarDataService.GetEnergyProducedAsync(lastMonthFrom, lastMonthTo, "pv")).EnergyKWh;
        var lastMonthCons = (await _solarDataService.GetEnergyProducedAsync(lastMonthFrom, lastMonthTo, "ac")).EnergyKWh;

        var improvement = lastMonthProd > 0 ? (totalProduced - lastMonthProd) / lastMonthProd * 100.0 : 0.0;
        
       
        var weekly = new WeeklyTrendsDto
        {
            EnergyProduced = Math.Round(totalProduced, 1),
            EnergyConsumed = Math.Round(totalConsumed, 1),
            Savings = Math.Round(totalProduced * 0.12, 0),
            Co2Avoided = Math.Round(totalProduced * 0.45, 1)
        };

        var monthly = new MonthlyComparisonDto
        {
            ThisMonth = Math.Round(totalProduced, 1),
            LastMonth = Math.Round(lastMonthProd, 1),
            Improvement = Math.Round(improvement, 1),
            BestDay = "N/A"
        };

        var totalDays = (to.Date - from.Date).Days;
        if (totalDays == 0) totalDays = 1;

        var dailyAverage = new DailyAverageDto
        {
            SolarGeneration = Math.Round(totalProduced / totalDays, 2),
            BatteryUsage = 0,
            Efficiency = totalProduced > 0 ? Math.Round((totalConsumed / totalProduced) * 100.0, 1) : 0,
            Uptime = 0
        };

        return new AnalyticsDataDto
        {
            DailyAverage = dailyAverage,
            WeeklyTrends = weekly,
            MonthlyComparison = monthly,
            Insights = new List<InsightDto>()
        };
    }

    private static (DateTime from, DateTime to, int gap) GetDateRange(string timeRange)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var todayEnd = todayStart.AddDays(1).AddTicks(-1);
        
        
        var dayOfWeek = (int)now.DayOfWeek;
        var startOfWeek = todayStart.AddDays(-(dayOfWeek == 0 ? 6 : dayOfWeek - 1));
        var endOfWeek = startOfWeek.AddDays(7).AddTicks(-1);

        return timeRange.ToLower() switch
        {
            "day" => (todayStart, now, 1),
            "week" => (startOfWeek, endOfWeek, 60),
            "month" => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1), 1),
            "year" => (new DateTime(now.Year, 1, 1), new DateTime(now.Year, 1, 1).AddYears(1).AddTicks(-1), 1),
            _ => (startOfWeek, endOfWeek, 1)
        };
    }

}
