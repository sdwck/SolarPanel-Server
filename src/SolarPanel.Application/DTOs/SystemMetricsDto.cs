namespace SolarPanel.Application.DTOs;

public class SystemMetricsDto
{
    public int TotalPanels { get; set; }
    public int ActivePanels { get; set; }
    public double TotalPowerGenerated { get; set; }
    public double AverageEfficiency { get; set; }
    public double TotalEnergyToday { get; set; }
    public double SystemUptime { get; set; }
}