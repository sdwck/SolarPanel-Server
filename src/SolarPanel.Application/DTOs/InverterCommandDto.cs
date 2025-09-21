namespace SolarPanel.Application.DTOs
{
    public class InverterCommandDto
    {
        public string CommandLoad { get; set; } = string.Empty;
        public string CommandCharge { get; set; } = string.Empty;
    }
}