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

        var email = configuration["Admin:Email"] ?? "admin@omnistay.local";
        var fullName = configuration["Admin:FullName"] ?? "OmniStay Admin";
        var password = configuration["Admin:SeedPassword"] ?? "Admin@123456";
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var seedAdminMatches = dbContext.Users
            .Where(item => item.Email == normalizedEmail)
            .OrderBy(item => item.Id)
            .ToArray();

        var user = seedAdminMatches.FirstOrDefault();

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
            foreach (var seedAdmin in seedAdminMatches)
            {
                seedAdmin.FullName = string.IsNullOrWhiteSpace(seedAdmin.FullName) ? fullName : seedAdmin.FullName;
                seedAdmin.Role = UserRoles.Admin;
                seedAdmin.AvatarUrl ??= string.Empty;
                if (seedAdmin.Balance <= 0)
                {
                    seedAdmin.Balance = 100_000_000m;
                }

                dbContext.Entry(seedAdmin).State = EntityState.Modified;
            }
        }

        dbContext.SaveChanges();
    }
}
