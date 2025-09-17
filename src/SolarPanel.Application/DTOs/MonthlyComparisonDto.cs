namespace SolarPanel.Application.DTOs;

public class MonthlyComparisonDto
{
    public double ThisMonth { get; set; }
    public double LastMonth { get; set; }
    public double Improvement { get; set; }
    public string BestDay { get; set; } = string.Empty;
}