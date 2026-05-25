namespace TheLeague.Shared.Infrastructure.Audit;

/// <summary>
/// Represents an immutable audit log entry recording a data mutation.
/// </summary>
public class AuditLogEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid? TenantId { get; init; }
    public string EntityType { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty; // Create, Update, Delete
    public string ActorId { get; init; } = string.Empty;
    public string? ActorName { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public string? BeforeValues { get; init; } // JSON, max 64KB
    public string? AfterValues { get; init; } // JSON, max 64KB
    public string? CorrelationId { get; init; }
}
