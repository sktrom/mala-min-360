namespace MalaMin.Api.Application.Common;

public interface ITenantContext
{
    Guid TenantId { get; }

    Guid UserId { get; }

    string UserRole { get; }

    bool IsAuthenticated { get; }
}
