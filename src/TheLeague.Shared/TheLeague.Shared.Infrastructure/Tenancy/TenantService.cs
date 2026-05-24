using TheLeague.Shared.Contracts.Services;

namespace TheLeague.Shared.Infrastructure.Tenancy;

public class TenantService : ITenantService
{
    public Guid? CurrentTenantId { get; private set; }

    public void SetCurrentTenant(Guid? tenantId)
    {
        CurrentTenantId = tenantId;
    }
}
