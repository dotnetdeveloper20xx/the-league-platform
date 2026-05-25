namespace TheLeague.Shared.Infrastructure.Audit;

/// <summary>
/// Service for managing data retention policies.
/// Handles notification of expiring data and deletion of expired data.
/// </summary>
public interface IDataRetentionService
{
    /// <summary>
    /// Checks for data approaching its retention expiry and sends 30-day pre-deletion notifications.
    /// </summary>
    /// <returns>The number of members notified about upcoming data deletion.</returns>
    Task<int> CheckAndNotifyExpiringDataAsync(CancellationToken ct = default);

    /// <summary>
    /// Deletes or anonymises data that has exceeded its configured retention period.
    /// </summary>
    /// <returns>The number of records deleted or anonymised.</returns>
    Task<int> DeleteExpiredDataAsync(CancellationToken ct = default);
}
