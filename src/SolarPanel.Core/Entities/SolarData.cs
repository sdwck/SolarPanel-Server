using System.ComponentModel.DataAnnotations;

namespace SolarPanel.Core.Entities;

public class SolarData
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    [MaxLength(50)]
    public string Command { get; set; } = string.Empty;
    [MaxLength(200)]
    public string CommandDescription { get; set; } = string.Empty;
    public int InverterHeatSinkTemperature { get; set; }
    public double BusVoltage { get; set; }
    public bool IsSbuPriorityVersionAdded { get; set; }
    public bool IsConfigurationChanged { get; set; }
    public bool IsSccFirmwareUpdated { get; set; }
    public bool IsLoadOn { get; set; }
    public bool IsBatteryVoltageToSteadyWhileCharging { get; set; }
    public bool IsChargingOn { get; set; }
    public bool IsSccChargingOn { get; set; }
    public bool IsAcChargingOn { get; set; }
    public bool IsChargingToFloat { get; set; }
    public bool IsSwitchedOn { get; set; }
    public bool IsReserved { get; set; }
    public int Rsv1 { get; set; }
    public int Rsv2 { get; set; }
    
    public virtual BatteryData? BatteryData { get; set; }
    public virtual PowerData? PowerData { get; set; }
}