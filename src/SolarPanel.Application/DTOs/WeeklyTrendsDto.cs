namespace SolarPanel.Application.DTOs;

public class WeeklyTrendsDto
{
    public double EnergyProduced { get; set; }
    public double EnergyConsumed { get; set; }
    public double Savings { get; set; }
    public double Co2Avoided { get; set; }
}