namespace TheLeague.Shared.Infrastructure.Audit;

/// <summary>
/// Service for recording and querying immutable audit log entries.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Records an audit log entry.
    /// </summary>
    Task LogAsync(AuditLogEntry entry, CancellationToken ct = default);

    /// <summary>
    /// Retrieves audit logs for a specific entity, ordered by timestamp descending.
    /// </summary>
    Task<List<AuditLogEntry>> GetLogsAsync(string entityType, string entityId, int page = 1, int pageSize = 20, CancellationToken ct = default);

    /// <summary>
    /// Retrieves audit logs by actor, ordered by timestamp descending.
    /// </summary>
    Task<List<AuditLogEntry>> GetLogsByActorAsync(string actorId, int page = 1, int pageSize = 20, CancellationToken ct = default);
}
