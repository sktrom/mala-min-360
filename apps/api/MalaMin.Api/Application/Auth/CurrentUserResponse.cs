namespace MalaMin.Api.Application.Auth;

public sealed record CurrentUserResponse(
    Guid Id,
    Guid TenantId,
    string FullName,
    string Email,
    string Role,
    string TenantName,
    string TenantSlug);
