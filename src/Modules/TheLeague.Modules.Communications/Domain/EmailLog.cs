using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Communications.Domain;

public class EmailLog : TenantEntity
{
    public Guid? RecipientMemberId { get; private set; }
    public string RecipientEmail { get; private set; } = string.Empty;
    public string TemplateType { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty; // Sent, Delivered, Bounced, Failed
    public DateTime SentAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string? FailureReason { get; private set; }

    private EmailLog() { }

    public static EmailLog Create(Guid clubId, Guid? recipientMemberId, string recipientEmail,
        string templateType, string subject, string status)
    {
        return new EmailLog
        {
            ClubId = clubId,
            RecipientMemberId = recipientMemberId,
            RecipientEmail = recipientEmail,
            TemplateType = templateType,
            Subject = subject,
            Status = status,
            SentAt = DateTime.UtcNow
        };
    }

    public void MarkDelivered()
    {
        Status = "Delivered";
        DeliveredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = "Failed";
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkBounced()
    {
        Status = "Bounced";
        UpdatedAt = DateTime.UtcNow;
    }

    public static readonly string[] ValidStatuses = { "Sent", "Delivered", "Bounced", "Failed" };
}
