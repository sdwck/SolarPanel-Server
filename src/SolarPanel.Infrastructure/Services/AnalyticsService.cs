using SolarPanel.Application.DTOs;
using SolarPanel.Application.Interfaces;

namespace SolarPanel.Infrastructure.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ISolarDataService _solarDataService;

    public AnalyticsService(ISolarDataService solarDataService)
    {
        _solarDataService = solarDataService;
    }

    public async Task<AnalyticsDataDto> GetAnalyticsDataAsync(string timeRange)
    {
        var (from, to, gap) = GetDateRange(timeRange);
        var (thisMonthFrom, thisMonthTo, thisMonthGap) = GetMonthRange(DateTime.UtcNow);
        var lastMonthFrom = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-1);
        var lastMonthTo = lastMonthFrom.AddMonths(1).AddTicks(-1);

        var currentData = (await _solarDataService.GetByDateRangeAsync(from, to, gap)).ToList();
        var thisMonthData = (await _solarDataService.GetByDateRangeAsync(thisMonthFrom, thisMonthTo, thisMonthGap))
            .ToList();
        var lastMonthData = (await _solarDataService.GetByDateRangeAsync(lastMonthFrom, lastMonthTo, thisMonthGap))
            .ToList();

        var currentEnergy = CalculateEnergy(currentData);
        var thisMonthEnergy = CalculateEnergy(thisMonthData);
        var lastMonthEnergy = CalculateEnergy(lastMonthData);

        var improvement = lastMonthEnergy.produced > 0
            ? (thisMonthEnergy.produced - lastMonthEnergy.produced) / lastMonthEnergy.produced * 100.0
            : 100.0;
        
        var firstRecord = currentData.FirstOrDefault();
        var lastRecord = currentData.LastOrDefault();

        var bestDay = CalculateBestDay(thisMonthData);
        var actualDays = (lastRecord?.Timestamp - firstRecord?.Timestamp ?? to - from).TotalDays;
        var batteryUsage = CalculateBatteryUsage(currentData);
        var uptime = CalculateUptime(currentData);
        var efficiency = currentEnergy.produced > 0
            ? Math.Min(100.0, currentEnergy.consumed / currentEnergy.produced * 100.0)
            : 0;

        return new AnalyticsDataDto
        {
            DailyAverage = new DailyAverageDto
            {
                SolarGeneration = Math.Round(currentEnergy.produced / actualDays, 2),
                BatteryUsage = Math.Round(batteryUsage, 1),
                Efficiency = Math.Round(efficiency, 1),
                Uptime = Math.Round(uptime, 1)
            },
            WeeklyTrends = new WeeklyTrendsDto
            {
                EnergyProduced = Math.Round(currentEnergy.produced, 1),
                EnergyConsumed = Math.Round(currentEnergy.consumed, 1),
                Savings = Math.Round(currentEnergy.produced * 0.12, 0),
                Co2Avoided = Math.Round(currentEnergy.produced * 0.45, 1)
            },
            MonthlyComparison = new MonthlyComparisonDto
            {
                ThisMonth = Math.Round(thisMonthEnergy.produced, 1),
                LastMonth = Math.Round(lastMonthEnergy.produced, 1),
                Improvement = Math.Round(improvement, 1),
                BestDay = bestDay
            },
            Insights = GenerateInsights(currentEnergy.produced, currentEnergy.consumed, efficiency, uptime)
        };
    }

    private static string CalculateBestDay(List<SolarDataDto> monthData)
    {
        if (monthData.Count == 0) return "N/A";

        var dailyProduction = monthData
            .Where(d => d.PowerData != null)
            .GroupBy(d => d.Timestamp.Date)
            .Select(g => new
            {
                Date = g.Key,
                Production = CalculateDailyEnergy(g.ToList())
            })
            .Where(x => x.Production > 0)
            .OrderByDescending(x => x.Production)
            .FirstOrDefault();

        return dailyProduction?.Date.ToString("MMM dd") ?? "N/A";
    }

    private static double CalculateDailyEnergy(List<SolarDataDto> dayData)
    {
        if (dayData.Count < 2) return 0;
        var data = dayData.OrderBy(x => x.Timestamp).ToList();

        var energyWh = 0.0;
        for (var i = 1; i < data.Count; i++)
        {
            var prev = data[i - 1];
            var curr = data[i];

            if (prev.PowerData?.PvInputPower < 0 || curr.PowerData?.PvInputPower < 0) continue;

            var hoursDiff = (curr.Timestamp - prev.Timestamp).TotalHours;
            energyWh += (prev.PowerData?.PvInputPower ?? 0 + curr.PowerData?.PvInputPower ?? 0) / 2.0 * hoursDiff;
        }

        return energyWh / 1000.0;
    }

    private static (double produced, double consumed) CalculateEnergy(List<SolarDataDto> data)
    {
        if (data.Count < 2) return (0, 0);

        var producedWh = 0.0;
        var consumedWh = 0.0;

        for (var i = 1; i < data.Count; i++)
        {
            var prev = data[i - 1];
            var curr = data[i];

            if (prev.PowerData == null || curr.PowerData == null) continue;

            var pvP1 = prev.PowerData.PvInputPower;
            var pvP2 = curr.PowerData.PvInputPower;
            var acP1 = prev.PowerData.AcOutputActivePower;
            var acP2 = curr.PowerData.AcOutputActivePower;

            if (pvP1 < 0 || pvP2 < 0 || acP1 < 0 || acP2 < 0) continue;

            var hoursDiff = (curr.Timestamp - prev.Timestamp).TotalHours;
            producedWh += (pvP1 + pvP2) / 2.0 * hoursDiff;
            consumedWh += (acP1 + acP2) / 2.0 * hoursDiff;
        }

        return (producedWh / 1000.0, consumedWh / 1000.0);
    }

    private static double CalculateBatteryUsage(List<SolarDataDto> dataList)
    {
        var energyWh = 0.0;

        var data = dataList
            .Where(d => d.BatteryData != null)
            .OrderBy(d => d.Timestamp)
            .ToList();

        for (var i = 1; i < data.Count; i++)
        {
            var prev = data[i - 1].BatteryData!;
            var curr = data[i].BatteryData!;
            var hoursDiff = (data[i].Timestamp - data[i - 1].Timestamp).TotalHours;

            var avgVoltage = (prev.BatteryVoltage + curr.BatteryVoltage) / 2.0m;
            var avgCurrent = (prev.BatteryChargingCurrent - prev.BatteryDischargeCurrent
                + curr.BatteryChargingCurrent - curr.BatteryDischargeCurrent) / 2.0m;

            energyWh += (double)(avgVoltage * avgCurrent * (decimal)hoursDiff);
        }

        return energyWh / 1000.0;
    }


    private static double CalculateUptime(List<SolarDataDto> dataList)
    {
        if (dataList.Count == 0) return 0.0;
        var onlineCount = dataList.Count(d => d.IsSwitchedOn);
        return (double)onlineCount / dataList.Count * 100.0;
    }

    private static List<InsightDto> GenerateInsights(double produced, double consumed, double efficiency, double uptime)
    {
        var insights = new List<InsightDto>();

        if (efficiency > 90)
            insights.Add(new InsightDto
            {
                Type = "positive",
                Title = "Excellent Efficiency",
                Description = "You're using most of your solar production effectively",
                Impact = "high"
            });
        else if (efficiency < 50)
            insights.Add(new InsightDto
            {
                Type = "warning",
                Title = "Low Efficiency",
                Description = "Consider optimizing energy usage during peak solar hours",
                Impact = "medium"
            });

        if (uptime > 95)
            insights.Add(new InsightDto
            {
                Type = "positive",
                Title = "High Reliability",
                Description = "System reliability is excellent with minimal downtime",
                Impact = "high"
            });
        else if (uptime < 90)
            insights.Add(new InsightDto
            {
                Type = "warning",
                Title = "Uptime Issues",
                Description = "System uptime could be improved - check for maintenance needs",
                Impact = "high"
            });

        if (produced > consumed)
            insights.Add(new InsightDto
            {
                Type = "positive",
                Title = "Energy Surplus",
                Description = "You're producing more energy than you consume - great job!",
                Impact = "high"
            });
        else if (consumed > produced * 1.5)
            insights.Add(new InsightDto
            {
                Type = "warning",
                Title = "High Consumption",
                Description = "Energy consumption is high - consider energy-saving measures",
                Impact = "medium"
            });

        return insights;
    }

    private static (DateTime from, DateTime to, int gap) GetDateRange(string timeRange)
    {
        var now = DateTime.UtcNow;

        return timeRange.ToLower() switch
        {
            "day" => (now.Date, now.Date.AddDays(1).AddTicks(-1), 5),
            "week" => GetWeekRange(now),
            "month" => GetMonthRange(now),
            "year" => GetYearRange(now),
            _ => GetWeekRange(now)
        };
    }

    private static (DateTime from, DateTime to, int gap) GetWeekRange(DateTime now)
    {
        var dayOfWeek = (int)now.DayOfWeek;
        var daysFromMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        var startOfWeek = now.Date.AddDays(-daysFromMonday);
        var endOfWeek = startOfWeek.AddDays(7).AddTicks(-1);

        return (startOfWeek, endOfWeek, 10);
    }

    private static (DateTime from, DateTime to, int gap) GetMonthRange(DateTime now)
    {
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

        return (startOfMonth, endOfMonth, 30);
    }

    private static (DateTime from, DateTime to, int gap) GetYearRange(DateTime now)
    {
        var startOfYear = new DateTime(now.Year, 1, 1);
        var endOfYear = startOfYear.AddYears(1).AddTicks(-1);

        return (startOfYear, endOfYear, 100);
    }
}