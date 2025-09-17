using SolarPanel.Application.DTOs;
using SolarPanel.Application.Interfaces;
using SolarPanel.Core.Interfaces;

namespace SolarPanel.Infrastructure.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ISolarDataRepository _repository;

    public AnalyticsService(ISolarDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<AnalyticsDataDto> GetAnalyticsDataAsync(string timeRange)
    {
        var (from, to, gap) = GetDateRange(timeRange);
        var raw = await _repository.GetByDateRangeAsync(from, to, gap);
        var data = raw
            .Where(d => d.PowerData != null)
            .ToArray();

        if (data.Length == 0)
            return new AnalyticsDataDto();

        double sumPv = 0;
        double sumAc = 0;
        double sumBatteryCap = 0;
        var batteryCount = 0;
        var switchedOnCount = 0;
        var totalCount = 0;

        var daily = new Dictionary<DateTime, (double pvSum, double acSum)>();

        foreach (var d in data)
        {
            var pv = d.PowerData!.PvInputPower;
            var ac = d.PowerData!.AcOutputActivePower;
            sumPv += pv;
            sumAc += ac;
            totalCount++;
            if (d.IsSwitchedOn) switchedOnCount++;
            if (d.BatteryData != null)
            {
                sumBatteryCap += d.BatteryData.BatteryCapacity;
                batteryCount++;
            }

            var day = d.Timestamp.Date;
            if (daily.TryGetValue(day, out var agg))
                daily[day] = (agg.pvSum + pv, agg.acSum + ac);
            else
                daily[day] = (pv, ac);
        }

        var totalEfficiency = 90.0; // TODO: Replace with real efficiency calculation

        var dailyAverage = new DailyAverageDto
        {
            SolarGeneration = Math.Round(totalCount > 0 ? sumPv / totalCount / 1000.0 : 0, 2),
            BatteryUsage = Math.Round(batteryCount > 0 ? (sumBatteryCap / batteryCount) / 100.0 * 10.0 : 0, 2),
            Efficiency = Math.Round(totalEfficiency, 1),
            Uptime = Math.Round(totalCount > 0 ? (double)switchedOnCount / totalCount * 100.0 : 0, 1)
        };

        var totalProduced = sumPv / 1000.0;
        var totalConsumed = sumAc / 1000.0;
        var weekly = new WeeklyTrendsDto
        {
            EnergyProduced = Math.Round(totalProduced, 1),
            EnergyConsumed = Math.Round(totalConsumed, 1),
            Savings = Math.Round(totalProduced * 0.12, 0),
            Co2Avoided = Math.Round(totalProduced * 0.45, 1)
        };

        var lastMonthFrom = from.AddMonths(-1);
        var lastMonthTo = to.AddMonths(-1);
        var lastMonthRaw = await _repository.GetByDateRangeAsync(lastMonthFrom, lastMonthTo);
        double lastMonthProd = 0;
        foreach (var d in lastMonthRaw)
        {
            if (d.PowerData != null)
                lastMonthProd += d.PowerData.PvInputPower;
        }

        lastMonthProd /= 1000.0;

        var improvement = lastMonthProd > 0 ? (totalProduced - lastMonthProd) / lastMonthProd * 100.0 : 0.0;
        var bestDayKv = daily.OrderByDescending(kv => kv.Value.pvSum).FirstOrDefault();
        var bestDay = bestDayKv.Key != default ? bestDayKv.Key.ToString("dd MMM") : "N/A";

        var monthly = new MonthlyComparisonDto
        {
            ThisMonth = Math.Round(totalProduced, 1),
            LastMonth = Math.Round(lastMonthProd, 1),
            Improvement = Math.Round(improvement, 1),
            BestDay = bestDay
        };

        var insights = new List<InsightDto>();
        if (bestDayKv.Key != default)
        {
            insights.Add(new InsightDto
            {
                Type = "positive",
                Title = "Peak Performance Day",
                Description =
                    $"{bestDayKv.Key:MMM dd} was your most productive day with {bestDayKv.Value.pvSum / 1000.0:F1} kWh generated",
                Impact = "high"
            });
        }

        var worstEff = daily
            .Where(kv => kv.Value.pvSum > 0)
            .Select(kv => new { Date = kv.Key, Ratio = 0.9 })
            .OrderBy(x => x.Ratio)
            .FirstOrDefault();

        if (worstEff != null && worstEff.Ratio < 0.8)
        {
            insights.Add(new InsightDto
            {
                Type = "warning",
                Title = "Efficiency Dip",
                Description = $"System efficiency dropped to {worstEff.Ratio * 100:F0}% on {worstEff.Date:MMM dd}",
                Impact = "medium"
            });
        }

        insights.Add(new InsightDto
        {
            Type = "info",
            Title = "Maintenance Reminder",
            Description = "Panel cleaning recommended to maintain optimal performance",
            Impact = "low"
        });

        return new AnalyticsDataDto
        {
            DailyAverage = dailyAverage,
            WeeklyTrends = weekly,
            MonthlyComparison = monthly,
            Insights = insights
        };
    }

    private static (DateTime from, DateTime to, int gap) GetDateRange(string timeRange)
    {
        var now = DateTime.UtcNow;
        return timeRange.ToLower() switch
        {
            "day" => (now.AddDays(-1), now, 10),
            "week" => (now.AddDays(-7), now, 60),
            "month" => (now.AddDays(-30), now, 120),
            "year" => (now.AddDays(-365), now, 240),
            _ => (now.AddDays(-7), now, 60)
        };
    }
}