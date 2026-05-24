namespace TheLeague.Shared.Domain.Entities;

public abstract class TenantEntity : BaseEntity
{
    public Guid ClubId { get; protected set; }
}
