namespace SolarPanel.Core.Entities;

public class ModeResult
{
    public int Id { get; set; }
    public string BatteryMode { get; set; } = string.Empty;
    public string LoadMode { get; set; } = string.Empty;
}
