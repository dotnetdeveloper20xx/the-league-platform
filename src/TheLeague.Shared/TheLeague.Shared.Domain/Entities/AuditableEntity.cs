namespace TheLeague.Shared.Domain.Entities;

public abstract class AuditableEntity : TenantEntity
{
    public string? CreatedBy { get; protected set; }
    public string? UpdatedBy { get; protected set; }
}
