# 16 — Communications & Campaigns

## 📖 Feature Overview

The Communications module handles all outbound messaging — transactional emails, SMS notifications, and bulk marketing campaigns. It uses a template engine with placeholder replacement, supports multiple delivery channels, and respects member communication preferences (opt-out).

### Key Capabilities
- 6 communication template types (Welcome, PasswordReset, PaymentReminder, etc.)
- Template engine with placeholder replacement ({{FirstName}}, {{ClubName}}, etc.)
- Email and SMS provider abstraction with mock implementations
- Bulk email campaigns (max 5000 recipients, respects opt-out)
- Delivery status tracking with logs (EmailLog, SmsLog)
- Retry logic (3× exponential backoff)
- Per-member channel preferences (email, SMS, push, in-app)

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| Template-based messaging | Consistent branding; non-technical staff can edit templates |
| Provider abstraction (interfaces) | Swap SendGrid/Twilio for mock in tests; multi-provider support |
| 5000 recipient cap on campaigns | Prevents accidental mass-send; aligns with ESP rate limits |
| Opt-out enforcement at send time | GDPR/PECR compliance; checked immediately before dispatch |
| Exponential backoff retry | Handles transient failures without overwhelming providers |
| Separate EmailLog/SmsLog | Different metadata per channel; independent audit trails |
| Mock providers for development | No real emails/SMS in dev; logs to console/database instead |

---

## 📊 Data Model

### CommunicationTemplate Entity
```csharp
public class CommunicationTemplate : TenantEntity
{
    public string Name { get; private set; }              // "Welcome Email"
    public TemplateType TemplateType { get; private set; }
    public string Subject { get; private set; }           // Email subject line
    public string Body { get; private set; }              // HTML/text with placeholders
    public string? SmsBody { get; private set; }          // SMS version (shorter)
    public bool IsActive { get; private set; }
    public string? Language { get; private set; }          // "en", "cy" (Welsh)
}

public enum TemplateType
{
    Welcome,              // New member welcome
    PasswordReset,        // Password reset link
    PaymentReminder,      // Upcoming/overdue payment
    BookingConfirmation,  // Session/facility booking confirmed
    EventNotification,    // Event updates and reminders
    MembershipRenewal     // Renewal reminder/confirmation
}
```

### Template Placeholders
```
Available placeholders (replaced at send time):
──────────────────────────────────────────────
{{FirstName}}        → Member's first name
{{LastName}}         → Member's last name
{{FullName}}         → First + Last name
{{Email}}            → Member's email address
{{ClubName}}         → Club name
{{ClubEmail}}        → Club contact email
{{MembershipType}}   → Membership type name
{{Amount}}           → Payment/fee amount
{{DueDate}}          → Payment due date
{{BookingDate}}      → Booking date/time
{{EventName}}        → Event name
{{EventDate}}        → Event date
{{ResetLink}}        → Password reset URL
{{LoginUrl}}         → Platform login URL
{{UnsubscribeLink}}  → Opt-out URL
```

### TemplateEngine
```csharp
public class TemplateEngine : ITemplateEngine
{
    public string Render(string template, Dictionary<string, string> placeholders)
    {
        var result = template;

        foreach (var (key, value) in placeholders)
        {
            result = result.Replace($"{{{{{key}}}}}", value ?? string.Empty);
        }

        // Remove any unreplaced placeholders (safety net)
        result = Regex.Replace(result, @"\{\{[^}]+\}\}", string.Empty);

        return result;
    }

    public List<string> ExtractPlaceholders(string template)
    {
        var matches = Regex.Matches(template, @"\{\{(\w+)\}\}");
        return matches.Select(m => m.Groups[1].Value).Distinct().ToList();
    }

    public ValidationResult Validate(string template, Dictionary<string, string> availablePlaceholders)
    {
        var used = ExtractPlaceholders(template);
        var unknown = used.Where(p => !availablePlaceholders.ContainsKey(p)).ToList();

        if (unknown.Any())
            return ValidationResult.Invalid($"Unknown placeholders: {string.Join(", ", unknown)}");

        return ValidationResult.Valid();
    }
}
```

---

## 📧 Email Provider

### IEmailProvider Interface
```csharp
public interface IEmailProvider
{
    Task<SendResult> SendAsync(EmailMessage message);
    Task<SendResult> SendBulkAsync(List<EmailMessage> messages);
    Task<DeliveryStatus> GetStatusAsync(string messageId);
}

public class EmailMessage
{
    public string To { get; set; }
    public string? From { get; set; }           // Defaults to club's configured sender
    public string Subject { get; set; }
    public string HtmlBody { get; set; }
    public string? PlainTextBody { get; set; }
    public string? ReplyTo { get; set; }
    public Dictionary<string, string>? Headers { get; set; }
}
```

### MockEmailProvider
```csharp
public class MockEmailProvider : IEmailProvider
{
    private readonly ILogger<MockEmailProvider> _logger;

    public Task<SendResult> SendAsync(EmailMessage message)
    {
        _logger.LogInformation(
            "MOCK EMAIL: To={To}, Subject={Subject}, Body length={Length}",
            message.To, message.Subject, message.HtmlBody?.Length ?? 0);

        return Task.FromResult(new SendResult
        {
            Success = true,
            MessageId = $"mock_email_{Guid.NewGuid():N}",
            SentAt = DateTime.UtcNow
        });
    }

    public Task<SendResult> SendBulkAsync(List<EmailMessage> messages)
    {
        _logger.LogInformation("MOCK BULK EMAIL: Sending {Count} messages", messages.Count);
        // Simulate sending each message
        var results = messages.Select(m => SendAsync(m));
        return Task.FromResult(new SendResult { Success = true });
    }
}
```

---

## 📱 SMS Provider

### ISmsProvider Interface
```csharp
public interface ISmsProvider
{
    Task<SendResult> SendAsync(SmsMessage message);
    Task<DeliveryStatus> GetStatusAsync(string messageId);
}

public class SmsMessage
{
    public string To { get; set; }              // Phone number (E.164 format)
    public string Body { get; set; }            // Max 160 chars for single SMS
    public string? From { get; set; }           // Sender ID or number
}
```

### MockSmsProvider
```csharp
public class MockSmsProvider : ISmsProvider
{
    private readonly ILogger<MockSmsProvider> _logger;

    public Task<SendResult> SendAsync(SmsMessage message)
    {
        _logger.LogInformation(
            "MOCK SMS: To={To}, Body={Body}",
            message.To, message.Body);

        return Task.FromResult(new SendResult
        {
            Success = true,
            MessageId = $"mock_sms_{Guid.NewGuid():N}",
            SentAt = DateTime.UtcNow
        });
    }
}
```

---

## 📢 Bulk Email Campaigns

### BulkEmailCampaign Entity
```csharp
public class BulkEmailCampaign : TenantEntity
{
    public string Name { get; private set; }              // "January Newsletter"
    public string Subject { get; private set; }
    public string HtmlContent { get; private set; }
    public Guid? TemplateId { get; private set; }         // Optional template base
    public CampaignStatus Status { get; private set; }
    public int TotalRecipients { get; private set; }      // Max 5000
    public int SentCount { get; private set; }
    public int FailedCount { get; private set; }
    public int OpenedCount { get; private set; }
    public DateTime? ScheduledAt { get; private set; }    // null = send immediately
    public DateTime? SentAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
}

public enum CampaignStatus
{
    Draft,          // Being composed
    Scheduled,      // Queued for future send
    Sending,        // Currently dispatching
    Completed,      // All messages sent
    Cancelled,      // Cancelled before completion
    Failed          // Critical failure during send
}
```

### Campaign Sending Logic
```csharp
public class CampaignSender
{
    private const int MaxRecipients = 5000;
    private const int BatchSize = 100;  // Send in batches of 100

    public async Task<CampaignResult> SendCampaign(BulkEmailCampaign campaign,
        List<CampaignRecipient> recipients)
    {
        // Enforce recipient limit
        if (recipients.Count > MaxRecipients)
            throw new InvalidOperationException(
                $"Campaign cannot exceed {MaxRecipients} recipients. Got: {recipients.Count}");

        // Filter out opted-out members
        var eligibleRecipients = recipients
            .Where(r => !r.HasOptedOut && r.EmailVerified)
            .ToList();

        campaign.UpdateStatus(CampaignStatus.Sending);
        campaign.TotalRecipients = eligibleRecipients.Count;

        // Send in batches
        foreach (var batch in eligibleRecipients.Chunk(BatchSize))
        {
            var messages = batch.Select(r => new EmailMessage
            {
                To = r.Email,
                Subject = campaign.Subject,
                HtmlBody = _templateEngine.Render(campaign.HtmlContent, r.Placeholders)
            }).ToList();

            await _emailProvider.SendBulkAsync(messages);
            campaign.SentCount += batch.Length;
        }

        campaign.UpdateStatus(CampaignStatus.Completed);
        return new CampaignResult(campaign.SentCount, campaign.FailedCount);
    }
}
```

---

## 📊 Delivery Logging

### EmailLog Entity
```csharp
public class EmailLog : TenantEntity
{
    public Guid? MemberId { get; private set; }
    public Guid? CampaignId { get; private set; }
    public string ToAddress { get; private set; }
    public string Subject { get; private set; }
    public string? ExternalMessageId { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public DateTime SentAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? OpenedAt { get; private set; }
    public DateTime? BouncedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public int AttemptCount { get; private set; }
}

public enum DeliveryStatus
{
    Queued,         // In send queue
    Sent,           // Dispatched to provider
    Delivered,      // Confirmed delivered
    Opened,         // Recipient opened (tracking pixel)
    Bounced,        // Hard bounce (invalid address)
    Failed,         // Delivery failed
    Rejected        // Rejected by provider (spam, etc.)
}
```

### SmsLog Entity
```csharp
public class SmsLog : TenantEntity
{
    public Guid? MemberId { get; private set; }
    public string ToNumber { get; private set; }
    public string Body { get; private set; }
    public string? ExternalMessageId { get; private set; }
    public DeliveryStatus Status { get; private set; }
    public DateTime SentAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public string? FailureReason { get; private set; }
    public int AttemptCount { get; private set; }
    public decimal? Cost { get; private set; }            // Per-SMS cost tracking
}
```

---

## 🔄 Retry Logic (Exponential Backoff)

```csharp
public class MessageRetryPolicy
{
    private const int MaxRetries = 3;
    private static readonly TimeSpan[] Delays = {
        TimeSpan.FromSeconds(5),    // 1st retry: 5 seconds
        TimeSpan.FromSeconds(25),   // 2nd retry: 25 seconds (5^2)
        TimeSpan.FromSeconds(125)   // 3rd retry: 125 seconds (5^3)
    };

    public async Task<SendResult> SendWithRetry(Func<Task<SendResult>> sendAction, string messageId)
    {
        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            var result = await sendAction();

            if (result.Success)
                return result;

            if (attempt < MaxRetries)
            {
                await Task.Delay(Delays[attempt]);
                _logger.LogWarning(
                    "Message {Id} failed attempt {Attempt}. Retrying in {Delay}s",
                    messageId, attempt + 1, Delays[attempt].TotalSeconds);
            }
        }

        return SendResult.Failed($"Failed after {MaxRetries + 1} attempts");
    }
}
```

**Retry schedule:**
| Attempt | Delay | Cumulative Time |
|---------|-------|-----------------|
| 1 (initial) | — | 0s |
| 2 (1st retry) | 5s | 5s |
| 3 (2nd retry) | 25s | 30s |
| 4 (3rd retry) | 125s | 155s (~2.5 min) |
| Give up | — | Mark as Failed |

---

## 🌐 API Endpoints

| Method | Route | Permission | Purpose |
|--------|-------|-----------|---------|
| GET | /api/v1/communications/templates | ManageMembers | List templates |
| POST | /api/v1/communications/templates | ManageMembers | Create template |
| PUT | /api/v1/communications/templates/{id} | ManageMembers | Update template |
| POST | /api/v1/communications/send | ManageMembers | Send single message |
| GET | /api/v1/communications/campaigns | ManageMembers | List campaigns |
| POST | /api/v1/communications/campaigns | ManageMembers | Create campaign |
| POST | /api/v1/communications/campaigns/{id}/send | ManageMembers | Send campaign |
| GET | /api/v1/communications/campaigns/{id}/stats | ManageMembers | Campaign statistics |
| GET | /api/v1/communications/logs/email | ManageMembers | Email delivery logs |
| GET | /api/v1/communications/logs/sms | ManageMembers | SMS delivery logs |
| PUT | /api/v1/members/{id}/preferences | ViewMembers | Update channel prefs |

---

## 🧪 Testing Approach

### Property Tests
```
Property 27: Opt-Out Enforcement
  For ANY bulk campaign, NO message SHALL be sent to a member
  who has opted out of that communication channel.

Property 28: Template Rendering Completeness
  For ANY rendered template, the output SHALL contain
  zero unreplaced placeholder patterns ({{...}}).

Property 29: Campaign Recipient Limit
  For ANY campaign, the recipient count SHALL not exceed 5000.
```

### Unit Tests
- Render template with all placeholders → all replaced
- Render template with missing placeholder → replaced with empty string
- Send to opted-out member → skipped, not sent
- Campaign with 5001 recipients → throws
- Retry on transient failure → succeeds on 2nd attempt
- Retry 3 times then fail → marked as Failed
- Mock email provider → logs message, returns success
- Validate template with unknown placeholder → returns invalid

---

## 🚀 How to Extend

### Adding push notifications:
1. Create `IPushProvider` interface
2. Implement with Firebase Cloud Messaging (FCM)
3. Add device token registration endpoint
4. Add `PushLog` entity for delivery tracking

### Adding email open/click tracking:
1. Embed tracking pixel in HTML emails (1x1 transparent GIF)
2. Wrap links with redirect URL for click tracking
3. Update `EmailLog.OpenedAt` when pixel is loaded
4. Record click events with link URL and timestamp
