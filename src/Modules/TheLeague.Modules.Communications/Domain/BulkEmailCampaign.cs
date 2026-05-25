using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Communications.Domain;

public class BulkEmailCampaign : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string? TargetSegment { get; private set; } // JSON string defining segment criteria
    public int TotalRecipients { get; private set; }
    public int SentCount { get; private set; }
    public int FailedCount { get; private set; }
    public int ExcludedCount { get; private set; }
    public string Status { get; private set; } = "Draft"; // Draft, Sending, Completed, Failed
    public DateTime? CompletedAt { get; private set; }

    private BulkEmailCampaign() { }

    public static BulkEmailCampaign Create(Guid clubId, string name, string subject, string body, string? targetSegment)
    {
        return new BulkEmailCampaign
        {
            ClubId = clubId,
            Name = name,
            Subject = subject,
            Body = body,
            TargetSegment = targetSegment,
            Status = "Draft"
        };
    }

    public void StartSending(int totalRecipients, int excludedCount)
    {
        Status = "Sending";
        TotalRecipients = totalRecipients;
        ExcludedCount = excludedCount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementSent()
    {
        SentCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementFailed()
    {
        FailedCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkCompleted()
    {
        Status = "Completed";
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        Status = "Failed";
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public const int MaxRecipients = 5000;

    public static readonly string[] ValidStatuses = { "Draft", "Sending", "Completed", "Failed" };
}
