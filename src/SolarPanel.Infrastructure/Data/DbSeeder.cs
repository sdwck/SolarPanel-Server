using Microsoft.Extensions.Configuration;
using SolarPanel.Core.Entities;

namespace SolarPanel.Infrastructure.Data;

public static class DbSeeder
{
    public static void SeedUser(this AppDbContext context, IConfiguration configuration)
    {
        if (context.Users.Any()) return;
        
        var userData = configuration.GetSection("UserData");
        var username = userData["Username"] ?? "admin";
        var email = userData["Email"] ?? "admin@solartrack.com";
        var password = userData["Password"] ?? "admin";

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = hashedPassword,
            IsActive = true,
            Roles = ["Admin"]
        };

        context.Users.Add(user);
        context.SaveChanges();
    }
}