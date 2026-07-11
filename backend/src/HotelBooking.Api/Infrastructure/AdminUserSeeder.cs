using HotelBooking.Api.Data;
using HotelBooking.Api.Models;
using HotelBooking.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Infrastructure;

public static class AdminUserSeeder
{
    public static void EnsureSeeded(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HotelBookingDbContext>();
        var passwordService = scope.ServiceProvider.GetRequiredService<PasswordService>();

        if (dbContext.Users.Any(user => user.Role == UserRoles.Admin))
        {
            return;
        }

        var email = configuration["Admin:Email"] ?? "admin@omnistay.local";
        var fullName = configuration["Admin:FullName"] ?? "OmniStay Admin";
        var password = configuration["Admin:SeedPassword"] ?? "Admin@123456";
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var user = dbContext.Users
            .SingleOrDefault(item => item.Email == normalizedEmail);

        if (user is null)
        {
            dbContext.Users.Add(new User
            {
                FullName = fullName,
                Email = normalizedEmail,
                PasswordHash = passwordService.Hash(password),
                Role = UserRoles.Admin,
                AvatarUrl = string.Empty,
                Balance = 100_000_000m,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            user.Role = UserRoles.Admin;
            user.AvatarUrl ??= string.Empty;
            if (user.Balance <= 0)
            {
                user.Balance = 100_000_000m;
            }
            dbContext.Entry(user).State = EntityState.Modified;
        }

        dbContext.SaveChanges();
    }
}
