namespace SolarPanel.Application.DTOs;

public class SolarDataDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Command { get; set; } = string.Empty;
    public string CommandDescription { get; set; } = string.Empty;
    public int InverterHeatSinkTemperature { get; set; }
    public double BusVoltage { get; set; }
    public bool IsLoadOn { get; set; }
    public bool IsChargingOn { get; set; }
    public bool IsSccChargingOn { get; set; }
    public bool IsAcChargingOn { get; set; }
    public bool IsSwitchedOn { get; set; }

    public BatteryDataDto? BatteryData { get; set; }
    public PowerDataDto? PowerData { get; set; }
}