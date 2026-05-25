namespace TheLeague.Shared.Infrastructure.Audit;

/// <summary>
/// Service for GDPR compliance operations including data export and erasure.
/// </summary>
public interface IGdprService
{
    /// <summary>
    /// Exports all PII data for a member within a club (GDPR Subject Access Request).
    /// Must be delivered within 72 hours.
    /// </summary>
    Task<GdprExportResult> ExportMemberDataAsync(Guid memberId, Guid clubId, CancellationToken ct = default);

    /// <summary>
    /// Anonymises PII for a member within a club (GDPR Right to Erasure).
    /// Financial aggregates are preserved for 7 years.
    /// </summary>
    Task<GdprErasureResult> EraseMemberDataAsync(Guid memberId, Guid clubId, CancellationToken ct = default);
}

/// <summary>
/// Result of a GDPR data export operation.
/// </summary>
/// <param name="Success">Whether the export was initiated successfully.</param>
/// <param name="ExportUrl">URL to download the exported data, if available.</param>
/// <param name="Error">Error message if the operation failed.</param>
public record GdprExportResult(bool Success, string? ExportUrl, string? Error);

/// <summary>
/// Result of a GDPR data erasure operation.
/// </summary>
/// <param name="Success">Whether the erasure was completed successfully.</param>
/// <param name="FieldsAnonymised">Number of PII fields that were anonymised.</param>
/// <param name="Error">Error message if the operation failed.</param>
public record GdprErasureResult(bool Success, int FieldsAnonymised, string? Error);
