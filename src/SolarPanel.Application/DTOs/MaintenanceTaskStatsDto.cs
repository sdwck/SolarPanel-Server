namespace SolarPanel.Application.DTOs;

public class MaintenanceTaskStatsDto
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    public int Overdue { get; set; }
    public decimal CompletionRate { get; set; }
}