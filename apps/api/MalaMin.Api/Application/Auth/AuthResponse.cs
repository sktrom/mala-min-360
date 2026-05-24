namespace MalaMin.Api.Application.Auth;

public sealed record AuthResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    CurrentUserResponse User);
