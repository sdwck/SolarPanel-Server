using System.ComponentModel.DataAnnotations.Schema;

namespace SolarPanel.Core.Entities;

public class BatteryData
{
    public int Id { get; set; }
    public int SolarDataId { get; set; }
    [Column(TypeName = "decimal(5,2)")]
    public decimal BatteryVoltage { get; set; }
    [Column(TypeName = "decimal(5,2)")]
    public decimal BatteryVoltageFromScc { get; set; }
    [Column(TypeName = "decimal(5,1)")]
    public decimal BatteryChargingCurrent { get; set; }
    public int BatteryCapacity { get; set; }
    [Column(TypeName = "decimal(5,1)")]
    public decimal BatteryDischargeCurrent { get; set; }
    
    [ForeignKey(nameof(SolarDataId))]
    public virtual SolarData SolarData { get; set; } = null!;
}