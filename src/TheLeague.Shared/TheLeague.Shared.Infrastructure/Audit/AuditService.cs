using System.Collections.Concurrent;

namespace TheLeague.Shared.Infrastructure.Audit;

/// <summary>
/// In-memory implementation of the audit service.
/// Will be backed by a dedicated database table in a future iteration.
/// </summary>
public class AuditService : IAuditService
{
    private readonly ConcurrentBag<AuditLogEntry> _entries = new();

    public Task LogAsync(AuditLogEntry entry, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _entries.Add(entry);
        return Task.CompletedTask;
    }

    public Task<List<AuditLogEntry>> GetLogsAsync(string entityType, string entityId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var results = _entries
            .Where(e => e.EntityType == entityType && e.EntityId == entityId)
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(results);
    }

    public Task<List<AuditLogEntry>> GetLogsByActorAsync(string actorId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var results = _entries
            .Where(e => e.ActorId == actorId)
            .OrderByDescending(e => e.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult(results);
    }
}
