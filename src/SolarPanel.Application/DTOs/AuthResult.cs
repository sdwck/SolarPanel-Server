using SolarPanel.Core.Entities;

namespace SolarPanel.Core.ValueObjects;

public class AuthResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? ErrorMessage { get; set; }
    public User? User { get; set; }
}