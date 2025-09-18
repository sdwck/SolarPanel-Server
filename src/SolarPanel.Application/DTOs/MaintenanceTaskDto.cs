using SolarPanel.Core.Entities;

namespace SolarPanel.Application.DTOs;

public class MaintenanceTaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MaintenanceTask.MaintenancePriority Priority { get; set; } = MaintenanceTask.MaintenancePriority.Medium;
    public MaintenanceTask.MaintenanceStatus Status { get; set; } = MaintenanceTask.MaintenanceStatus.Pending;
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public MaintenanceTask.MaintenanceCategory Category { get; set; } = MaintenanceTask.MaintenanceCategory.Other;
    public int? EstimatedDuration { get; set; }
    public string? AssignedTo { get; set; }
    public string? Notes { get; set; }
    public List<string> Tags { get; set; } = [];
}