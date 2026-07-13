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
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public ActionResult<AuthResponse> Register(RegisterRequest request)
    {
        var email = NormalizeEmail(request.Email);
        if (dbContext.Users.Any(user => user.Email == email))
        {
            return Conflict(new { error = "Email already exists." });
        }

        var role = NormalizeRegistrationRole(request.Role);
        if (role is null)
        {
            return BadRequest(new { error = "Role must be Customer or HotelOwner." });
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = passwordService.Hash(request.Password),
            Role = role,
            Balance = 100_000_000m,
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
        var user = dbContext.Users
            .AsNoTracking()
            .SingleOrDefault(item => item.Id == CurrentUserId());

        if (user is null)
        {
            return NotFound(new { error = "User was not found." });
        }

        return Ok(ToSummary(user));
    }

    [Authorize]
    [HttpPost("refresh")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AuthResponse> Refresh()
    {
        var user = dbContext.Users
            .AsNoTracking()
            .SingleOrDefault(item => item.Id == CurrentUserId());

        if (user is null)
        {
            return NotFound(new { error = "User was not found." });
        }

        var summary = ToSummary(user);
        return Ok(new AuthResponse(jwtTokenService.CreateToken(summary), summary));
    }

    [Authorize]
    [HttpPut("me")]
    [HttpPost("me")]
    [ProducesResponseType<UserSummary>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserSummary> UpdateMe(UpdateProfileRequest request)
    {
        var user = dbContext.Users.SingleOrDefault(item => item.Id == CurrentUserId());
        if (user is null)
        {
            return NotFound(new { error = "User was not found." });
        }

        user.FullName = request.FullName.Trim();
        user.AvatarUrl = request.AvatarUrl?.Trim() ?? string.Empty;
        dbContext.SaveChanges();

        return Ok(ToSummary(user));
    }

    [Authorize]
    [HttpPut("me/bank-account")]
    [HttpPost("me/bank-account")]
    [ProducesResponseType<UserSummary>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UserSummary> UpdateBankAccount(UpdateBankAccountRequest request)
    {
        var user = dbContext.Users.SingleOrDefault(item => item.Id == CurrentUserId());
        if (user is null)
        {
            return NotFound(new { error = "User was not found." });
        }

        user.BankName = request.BankName?.Trim() ?? string.Empty;
        user.BankAccountNumber = request.BankAccountNumber?.Trim() ?? string.Empty;
        user.BankAccountHolder = request.BankAccountHolder?.Trim() ?? string.Empty;
        dbContext.SaveChanges();

        return Ok(ToSummary(user));
    }

    [Authorize]
    [HttpPut("me/password")]
    [HttpPost("me/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ChangePassword(ChangePasswordRequest request)
    {
        var user = dbContext.Users.SingleOrDefault(item => item.Id == CurrentUserId());
        if (user is null)
        {
            return NotFound(new { error = "User was not found." });
        }

        if (!passwordService.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest(new { error = "Current password is incorrect." });
        }

        user.PasswordHash = passwordService.Hash(request.NewPassword);
        dbContext.SaveChanges();

        return NoContent();
    }

    private int CurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string? NormalizeRegistrationRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role) || role.Equals(UserRoles.Customer, StringComparison.OrdinalIgnoreCase))
        {
            return UserRoles.Customer;
        }

        if (role.Equals(UserRoles.HotelOwner, StringComparison.OrdinalIgnoreCase))
        {
            return UserRoles.HotelOwner;
        }

        return null;
    }

    private static UserSummary ToSummary(User user)
    {
        return new UserSummary(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.AvatarUrl,
            user.BankName,
            user.BankAccountNumber,
            user.BankAccountHolder,
            user.Balance);
    }
}
