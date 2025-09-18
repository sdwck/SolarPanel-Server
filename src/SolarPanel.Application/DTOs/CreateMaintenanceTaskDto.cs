using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SolarPanel.Core.Entities;

namespace SolarPanel.Application.DTOs;

public class CreateMaintenanceTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MaintenanceTask.MaintenancePriority Priority { get; set; } = MaintenanceTask.MaintenancePriority.Medium;
    public DateTime DueDate { get; set; }
    public MaintenanceTask.MaintenanceCategory Category { get; set; } = MaintenanceTask.MaintenanceCategory.Other;
    public int? EstimatedDuration { get; set; }
    public string? AssignedTo { get; set; }
    public string? Notes { get; set; }
    public List<string> Tags { get; set; } = [];
}