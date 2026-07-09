using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HotelBooking.Api.Contracts;
using Microsoft.IdentityModel.Tokens;

namespace HotelBooking.Api.Services;

public sealed class JwtTokenService(IConfiguration configuration)
{
    private const string DefaultIssuer = "AWSOmniStay";
    private const string DefaultAudience = "AWSOmniStay.Web";

    public string CreateToken(UserSummary user)
    {
        var key = CreateSecurityKey(configuration);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var issuer = configuration["Jwt:Issuer"] ?? DefaultIssuer;
        var audience = configuration["Jwt:Audience"] ?? DefaultAudience;
        var expires = DateTime.UtcNow.AddHours(configuration.GetValue("Jwt:ExpireHours", 12));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static SymmetricSecurityKey CreateSecurityKey(IConfiguration configuration)
    {
        var secret = configuration["Jwt:Secret"]
            ?? "local-development-secret-change-before-aws-deploy-32chars";
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
    }
}
