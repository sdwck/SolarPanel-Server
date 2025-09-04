namespace SolarPanel.Application.DTOs;

public class BatteryDataDto
{
    public decimal BatteryVoltage { get; set; }
    public decimal BatteryChargingCurrent { get; set; }
    public int BatteryCapacity { get; set; }
    public decimal BatteryDischargeCurrent { get; set; }
}