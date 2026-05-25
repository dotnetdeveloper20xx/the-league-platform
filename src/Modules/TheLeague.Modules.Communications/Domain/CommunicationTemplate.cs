using TheLeague.Shared.Domain.Entities;

namespace TheLeague.Modules.Communications.Domain;

public class CommunicationTemplate : TenantEntity
{
    public string Name { get; private set; } = string.Empty;
    public string TemplateType { get; private set; } = string.Empty; // Welcome, PasswordReset, PaymentReminder, BookingConfirmation, EventNotification, MembershipRenewal
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    private CommunicationTemplate() { }

    public static CommunicationTemplate Create(Guid clubId, string name, string templateType, string subject, string body)
    {
        return new CommunicationTemplate
        {
            ClubId = clubId,
            Name = name,
            TemplateType = templateType,
            Subject = subject,
            Body = body,
            IsActive = true
        };
    }

    public void Update(string name, string templateType, string subject, string body, bool isActive)
    {
        Name = name;
        TemplateType = templateType;
        Subject = subject;
        Body = body;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public static readonly string[] ValidTemplateTypes =
    {
        "Welcome", "PasswordReset", "PaymentReminder",
        "BookingConfirmation", "EventNotification", "MembershipRenewal"
    };
}
