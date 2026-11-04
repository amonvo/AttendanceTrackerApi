using System.ComponentModel.DataAnnotations;
using AttendanceTrackerApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace AttendanceTrackerApi.Data;
public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        var db = services.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        if (await db.Users.AnyAsync(u => u.Role == "admin")) return;
        var name = configuration["Bootstrap:Username"]?.Trim();
        var email = configuration["Bootstrap:Email"]?.Trim();
        var password = configuration["Bootstrap:Password"];
        if (string.IsNullOrWhiteSpace(name) || name.Length is < 3 or > 80 ||
            string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email) ||
            password is null || password.Length is < 12 or > 128)
            throw new InvalidOperationException("Set Bootstrap__Username, Bootstrap__Email and a 12–128 character Bootstrap__Password to initialize the first administrator.");
        if (await db.Users.AnyAsync(u => u.Username == name || u.Email == email))
            throw new InvalidOperationException("Bootstrap identity already exists. Review existing users before creating an administrator.");
        var user = new User { Username = name, Email = email, Role = "admin", Position = "Administrator" };
        user.PasswordHash = services.GetRequiredService<IPasswordHasher<User>>().HashPassword(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}
