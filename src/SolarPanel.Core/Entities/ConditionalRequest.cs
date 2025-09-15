using System.ComponentModel.DataAnnotations;

namespace SolarPanel.Core.Entities;

public class ConditionalRequest
{
    [MaxLength(200)]
    public string? IfNoneMatch { get; init; }
    [MaxLength(50)]
    public string? IfModifiedSince { get; init; }
}