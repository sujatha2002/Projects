using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FoodieCart.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace FoodieCart.Api.Services;

public class JwtTokenService(IConfiguration config)
{
    public AuthTokenResult GenerateToken(ApplicationUser user, IList<string> roles)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(double.Parse(config["Jwt:ExpireHours"] ?? "24"));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.Name, user.FullName ?? user.Email ?? ""),
            new("dietary", user.DietaryPreference ?? "None")
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new AuthTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}

public record AuthTokenResult(string Token, DateTime ExpiresAt);
