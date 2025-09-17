namespace SolarPanel.Application.DTOs;

public class PredictionDataDto
{
    public string Period { get; set; } = string.Empty;
    public double EnergyKWh { get; set; }
    public double Confidence { get; set; }
    public List<string> Factors { get; set; } = [];
}