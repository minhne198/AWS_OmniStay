using System.Security.Claims;
using HotelBooking.Api.Contracts;
using HotelBooking.Api.Data;
using HotelBooking.Api.Models;
using HotelBooking.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    HotelBookingDbContext dbContext,
    PasswordService passwordService,
    JwtTokenService jwtTokenService) : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<AuthResponse> Register(RegisterRequest request)
    {
        var email = NormalizeEmail(request.Email);
        if (dbContext.Users.Any(user => user.Email == email))
        {
            return Conflict(new { error = "Email already exists." });
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = passwordService.Hash(request.Password),
            Role = UserRoles.Customer,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Users.Add(user);
        dbContext.SaveChanges();

        var summary = ToSummary(user);
        return Created("/api/auth/me", new AuthResponse(jwtTokenService.CreateToken(summary), summary));
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<AuthResponse> Login(LoginRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var user = dbContext.Users.SingleOrDefault(item => item.Email == email);
        if (user is null || !passwordService.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { error = "Email or password is invalid." });
        }

        var summary = ToSummary(user);
        return Ok(new AuthResponse(jwtTokenService.CreateToken(summary), summary));
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<UserSummary>(StatusCodes.Status200OK)]
    public ActionResult<UserSummary> Me()
    {
        return Ok(new UserSummary(
            CurrentUserId(),
            User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            User.FindFirstValue(ClaimTypes.Role) ?? UserRoles.Customer));
    }

    private int CurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static UserSummary ToSummary(User user)
    {
        return new UserSummary(user.Id, user.FullName, user.Email, user.Role);
    }
}
