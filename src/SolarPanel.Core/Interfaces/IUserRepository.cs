using SolarPanel.Core.Entities;

namespace SolarPanel.Core.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByCredentialAsync(string credential);
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task UpdateRefreshTokenAsync(int userId, string refreshToken, DateTime expiryTime);
    Task RevokeRefreshTokenAsync(int userId);
    Task<List<User>> GetAllUsersAsync();
    Task<User> CreateUserAsync(User user);
}