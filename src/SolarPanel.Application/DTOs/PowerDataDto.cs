namespace SolarPanel.Application.DTOs;

public class PowerDataDto
{
    public decimal AcInputVoltage { get; set; }
    public decimal AcInputFrequency { get; set; }
    public decimal AcOutputVoltage { get; set; }
    public decimal AcOutputFrequency { get; set; }
    public int AcOutputActivePower { get; set; }
    public int AcOutputLoad { get; set; }
    public decimal PvInputVoltage { get; set; }
    public decimal PvInputCurrent { get; set; }
    public int PvInputPower { get; set; }
}