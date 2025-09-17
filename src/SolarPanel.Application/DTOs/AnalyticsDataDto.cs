namespace SolarPanel.Application.DTOs;

public class AnalyticsDataDto
{
    public DailyAverageDto DailyAverage { get; set; } = new();
    public WeeklyTrendsDto WeeklyTrends { get; set; } = new();
    public MonthlyComparisonDto MonthlyComparison { get; set; } = new();
    public List<InsightDto> Insights { get; set; } = new();
}