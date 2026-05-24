namespace MalaMin.Api.Application.Tenants;

public sealed record TenantResponse(
    Guid Id,
    string Name,
    string Slug,
    string? Phone,
    string? WhatsAppNumber,
    string? LogoUrl,
    string? Address,
    string? City,
    string Status);
