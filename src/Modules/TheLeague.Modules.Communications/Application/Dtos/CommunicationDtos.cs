namespace TheLeague.Modules.Communications.Application.Dtos;

public record TemplateDto(
    Guid Id,
    string Name,
    string TemplateType,
    string Subject,
    string Body,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record EmailLogDto(
    Guid Id,
    Guid? RecipientMemberId,
    string RecipientEmail,
    string TemplateType,
    string Subject,
    string Status,
    DateTime SentAt,
    DateTime? DeliveredAt,
    string? FailureReason
);

public record BulkEmailCampaignDto(
    Guid Id,
    string Name,
    string Subject,
    string Body,
    string? TargetSegment,
    int TotalRecipients,
    int SentCount,
    int FailedCount,
    int ExcludedCount,
    string Status,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

public record SmsLogDto(
    Guid Id,
    Guid? RecipientMemberId,
    string RecipientPhone,
    string Message,
    string Status,
    DateTime SentAt,
    string? FailureReason
);
