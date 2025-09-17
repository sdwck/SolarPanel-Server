namespace SolarPanel.Application.DTOs;

public class HistoryDataDto
{
    public DateTimeOffset Timestamp { get; set; }
    public double SolarInput { get; set; }
    public double BatteryLevel { get; set; }
    public double PowerOutput { get; set; }
    public double Temperature { get; set; }
    public double Efficiency { get; set; }
    public string Status { get; set; } = string.Empty;
}