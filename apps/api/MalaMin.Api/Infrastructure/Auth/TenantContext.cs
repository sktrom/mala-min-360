using System.Security.Claims;
using MalaMin.Api.Application.Common;

namespace MalaMin.Api.Infrastructure.Auth;

public sealed class TenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public Guid TenantId => GetRequiredGuidClaim("tenant_id", "Tenant id claim is missing or invalid.");

    public Guid UserId
    {
        get
        {
            var value = User?.FindFirstValue("sub")
                ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!IsAuthenticated || !Guid.TryParse(value, out var userId))
            {
                throw new UnauthorizedAccessException("User id claim is missing or invalid.");
            }

            return userId;
        }
    }

    public string UserRole
    {
        get
        {
            var value = User?.FindFirstValue("role")
                ?? User?.FindFirstValue(ClaimTypes.Role);

            if (!IsAuthenticated || string.IsNullOrWhiteSpace(value))
            {
                throw new UnauthorizedAccessException("User role claim is missing.");
            }

            return value;
        }
    }

    private Guid GetRequiredGuidClaim(string claimType, string errorMessage)
    {
        var value = User?.FindFirstValue(claimType);

        if (!IsAuthenticated || !Guid.TryParse(value, out var claimValue))
        {
            throw new UnauthorizedAccessException(errorMessage);
        }

        return claimValue;
    }
}
