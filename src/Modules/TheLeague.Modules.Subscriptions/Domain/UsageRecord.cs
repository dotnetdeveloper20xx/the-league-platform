using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Subscriptions.Domain;

public class UsageRecord : BaseEntity
{
    public Guid ClubId { get; private set; }
    public int CurrentMemberCount { get; private set; }
    public long CurrentStorageBytes { get; private set; }
    public int CurrentMonthlySmsUsed { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }

    public static UsageRecord Create(Guid clubId) => new()
    {
        ClubId = clubId,
        PeriodStart = DateTime.UtcNow,
        PeriodEnd = DateTime.UtcNow.AddMonths(1)
    };

    public void IncrementMembers() => CurrentMemberCount++;
    public void DecrementMembers() => CurrentMemberCount = Math.Max(0, CurrentMemberCount - 1);
    public void AddStorage(long bytes) => CurrentStorageBytes += bytes;
    public void RemoveStorage(long bytes) => CurrentStorageBytes = Math.Max(0, CurrentStorageBytes - bytes);
    public void IncrementSms() => CurrentMonthlySmsUsed++;
}
