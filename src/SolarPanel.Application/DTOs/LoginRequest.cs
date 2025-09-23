namespace SolarPanel.Application.DTOs;

public class LoginRequest
{
    public string Credential { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}