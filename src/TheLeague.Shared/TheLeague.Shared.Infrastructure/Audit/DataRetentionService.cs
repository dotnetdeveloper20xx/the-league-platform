namespace TheLeague.Shared.Infrastructure.Audit;

/// <summary>
/// Placeholder data retention service implementation.
/// Will be implemented with Hangfire scheduled jobs in a future iteration.
/// </summary>
public class DataRetentionService : IDataRetentionService
{
    public Task<int> CheckAndNotifyExpiringDataAsync(CancellationToken ct = default)
    {
        // Placeholder: no data to check yet.
        // Real implementation will query inactive members based on configurable inactivity period
        // and send 30-day pre-deletion notifications.
        return Task.FromResult(0);
    }

    public Task<int> DeleteExpiredDataAsync(CancellationToken ct = default)
    {
        // Placeholder: no data to delete yet.
        // Real implementation will anonymise/delete data that has exceeded its retention period.
        return Task.FromResult(0);
    }
}
