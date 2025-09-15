using System.ComponentModel.DataAnnotations;

namespace SolarPanel.Core.Entities;

public class ScriptMetadata
{
    [MaxLength(100)]
    public string ETag { get; init; } = string.Empty;
    public DateTime LastWriteUtc { get; init; }
    [MaxLength(128)]
    public string Signature { get; init; } = string.Empty;
    [MaxLength(500)]
    public string FilePath { get; init; } = string.Empty;
}