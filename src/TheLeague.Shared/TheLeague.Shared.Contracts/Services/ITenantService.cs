namespace TheLeague.Shared.Contracts.Services;

public interface ITenantService
{
    Guid? CurrentTenantId { get; }
    void SetCurrentTenant(Guid? tenantId);
}
