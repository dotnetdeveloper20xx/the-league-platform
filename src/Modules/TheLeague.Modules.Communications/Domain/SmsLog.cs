using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Communications.Domain;

public class SmsLog : TenantEntity
{
    public Guid? RecipientMemberId { get; private set; }
    public string RecipientPhone { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty; // Sent, Delivered, Failed
    public DateTime SentAt { get; private set; }
    public string? FailureReason { get; private set; }

    private SmsLog() { }

    public static SmsLog Create(Guid clubId, Guid? recipientMemberId, string recipientPhone,
        string message, string status)
    {
        return new SmsLog
        {
            ClubId = clubId,
            RecipientMemberId = recipientMemberId,
            RecipientPhone = recipientPhone,
            Message = message,
            Status = status,
            SentAt = DateTime.UtcNow
        };
    }

    public void MarkDelivered()
    {
        Status = "Delivered";
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = "Failed";
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public const int MaxMessageLength = 160;

    public static readonly string[] ValidStatuses = { "Sent", "Delivered", "Failed" };
}
