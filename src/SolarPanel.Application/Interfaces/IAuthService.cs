using SolarPanel.Core.Entities;
using SolarPanel.Core.ValueObjects;

namespace SolarPanel.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string credential, string password);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<User?> GetUserByUsernameAsync(string credential);
}