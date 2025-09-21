namespace SolarPanel.Application.DTOs
{
    public class SolarRadiationForecastDto
    {
        public DateTime Date { get; set; }
        public double Radiation { get; set; } 
        public double EstimatedEnergy { get; set; } 
    }
}

