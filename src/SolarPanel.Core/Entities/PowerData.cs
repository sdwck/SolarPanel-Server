using System.ComponentModel.DataAnnotations.Schema;

namespace SolarPanel.Core.Entities;

public class PowerData
{
    public int Id { get; set; }
    public int SolarDataId { get; set; }
    [Column(TypeName = "decimal(6,2)")]
    public decimal AcInputVoltage { get; set; }
    [Column(TypeName = "decimal(4,1)")]
    public decimal AcInputFrequency { get; set; }
    [Column(TypeName = "decimal(6,2)")]
    public decimal AcOutputVoltage { get; set; }
    [Column(TypeName = "decimal(4,1)")]
    public decimal AcOutputFrequency { get; set; }
    public int AcOutputApparentPower { get; set; }
    public int AcOutputActivePower { get; set; }
    public int AcOutputLoad { get; set; }
    [Column(TypeName = "decimal(5,1)")]
    public decimal PvInputCurrent { get; set; }
    [Column(TypeName = "decimal(6,2)")]
    public decimal PvInputVoltage { get; set; }
    public int PvInputPower { get; set; }
    
    [ForeignKey(nameof(SolarDataId))]
    public virtual SolarData SolarData { get; set; } = null!;
}