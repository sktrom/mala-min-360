using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MalaMin.Api.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace MalaMin.Api.Infrastructure.Auth;

public sealed class JwtTokenService(IConfiguration configuration)
{
    public JwtTokenResult GenerateToken(AppUser user)
    {
        var issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT issuer is not configured.");
        var audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT audience is not configured.");
        var signingKey = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("JWT signing key is not configured.");
        var accessTokenMinutes = configuration.GetValue<int>("Jwt:AccessTokenMinutes");

        if (accessTokenMinutes <= 0)
        {
            throw new InvalidOperationException("JWT access token lifetime must be greater than zero.");
        }

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(accessTokenMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("role", user.Role),
            new Claim("full_name", user.FullName)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new JwtTokenResult(accessToken, expiresAt);
    }
}

public sealed record JwtTokenResult(string AccessToken, DateTimeOffset ExpiresAt);
