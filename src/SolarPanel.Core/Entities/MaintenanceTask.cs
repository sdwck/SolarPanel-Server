using System.ComponentModel.DataAnnotations;

namespace SolarPanel.Core.Entities;

public class MaintenanceTask
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] [MaxLength(200)] public string Title { get; set; } = string.Empty;

    [Required] [MaxLength(1000)] public string Description { get; set; } = string.Empty;

    [Required] [MaxLength(20)] public MaintenancePriority Priority { get; set; } = MaintenancePriority.Medium;

    [Required] [MaxLength(20)] public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Pending;
    public DateTime DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    [Required] [MaxLength(50)] public MaintenanceCategory Category { get; set; } = MaintenanceCategory.Other;
    public int? EstimatedDuration { get; set; }

    [MaxLength(100)] public string? AssignedTo { get; set; }

    [MaxLength(2000)] public string? Notes { get; set; }

    public List<string> Tags { get; set; } = [];

    public enum MaintenancePriority
    {
        Low,
        Medium,
        High
    }

    public enum MaintenanceStatus
    {
        Pending,
        InProgress,
        Completed,
        Overdue
    }

    public enum MaintenanceCategory
    {
        Cleaning,
        Inspection,
        Repair,
        Upgrade,
        Calibration,
        Other
    }
}