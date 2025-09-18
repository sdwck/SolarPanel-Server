using SolarPanel.Core.Entities;

namespace SolarPanel.Application.DTOs;

public class UpdateMaintenanceTaskDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public MaintenanceTask.MaintenancePriority? Priority { get; set; }
    public MaintenanceTask.MaintenanceStatus? Status { get; set; }
    public DateTime? DueDate { get; set; }
    public MaintenanceTask.MaintenanceCategory? Category { get; set; }
    public int? EstimatedDuration { get; set; }
    public string? AssignedTo { get; set; }
    public string? Notes { get; set; }
    public List<string>? Tags { get; set; }
}