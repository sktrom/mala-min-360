using MalaMin.Api.Domain.Entities;
using MalaMin.Api.Infrastructure.Auth;
using MalaMin.Api.Infrastructure.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MalaMin.Api.Application.Auth;

public sealed class AuthService(
    AppDbContext db,
    IPasswordHasher<AppUser> passwordHasher,
    JwtTokenService jwtTokenService)
{
    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users
            .Include(appUser => appUser.Tenant)
            .SingleOrDefaultAsync(appUser => appUser.Email.ToLower() == email, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        var passwordResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var token = jwtTokenService.GenerateToken(user);

        return new AuthResponse(
            token.AccessToken,
            token.ExpiresAt,
            CreateCurrentUserResponse(user));
    }

    public static CurrentUserResponse CreateCurrentUserResponse(AppUser user)
    {
        return new CurrentUserResponse(
            user.Id,
            user.TenantId,
            user.FullName,
            user.Email,
            user.Role,
            user.Tenant.Name,
            user.Tenant.Slug);
    }
}
