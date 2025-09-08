namespace SolarPanel.Application.DTOs;

public class EnergyResponseDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public double EnergyKWh { get; set; }
    public int SamplesUsed { get; set; }
    public string Source { get; set; } = "pv";

}