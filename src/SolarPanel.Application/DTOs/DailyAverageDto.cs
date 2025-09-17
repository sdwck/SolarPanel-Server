namespace SolarPanel.Application.DTOs;

public class DailyAverageDto
{
    public double SolarGeneration { get; set; }
    public double BatteryUsage { get; set; }
    public double Efficiency { get; set; }
    public double Uptime { get; set; }
}