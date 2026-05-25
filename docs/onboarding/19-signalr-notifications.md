# 19 — Real-Time Notifications & SignalR

## 📖 Feature Overview

The Notifications module provides real-time communication using SignalR hubs for instant in-app updates and live match commentary. It supports multi-channel delivery (in-app, email, SMS, push), per-member channel preferences, webhook integrations for external systems, and retry logic for failed deliveries.

### Key Capabilities
- NotificationHub (authorized, club-group + user-group membership)
- MatchCentreHub (public, match-group subscriptions)
- Multi-channel delivery (in-app ≤2s, email/SMS/push ≤30s)
- INotificationService (SendToUser, SendToClub, SendToMatch)
- IWebhookService for external integrations
- Per-member channel preferences
- Retry logic (3× exponential backoff: 5s, 25s, 125s)
- JWT token from query string for SignalR authentication

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| Two separate hubs | NotificationHub is private (auth required); MatchCentreHub is public |
| Group-based messaging | Efficient: send to club/match group, not individual connections |
| JWT via query string | WebSocket connections can't use Authorization header; standard pattern |
| 2s SLA for in-app | Users expect instant feedback; SignalR delivers sub-second |
| 30s SLA for email/SMS/push | External providers have latency; 30s is acceptable |
| Per-member preferences | GDPR compliance; members control how they're contacted |
| Webhooks for integrations | External systems (scoreboard apps, websites) can subscribe |
| Exponential backoff (5/25/125s) | Handles transient failures without overwhelming services |

---

## 📊 Data Model

### Notification Entity
```csharp
public class Notification : TenantEntity
{
    public Guid? RecipientMemberId { get; private set; }  // null = broadcast to club
    public string Title { get; private set; }
    public string Body { get; private set; }
    public NotificationType Type { get; private set; }
    public NotificationPriority Priority { get; private set; }
    public string? ActionUrl { get; private set; }        // Deep link in app
    public string? Data { get; private set; }             // JSON payload for rich notifications
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }       // Auto-dismiss after this
    public List<DeliveryChannel> DeliveredVia { get; private set; }
}

public enum NotificationType
{
    PaymentReceived,
    PaymentFailed,
    BookingConfirmed,
    BookingCancelled,
    SessionReminder,
    EventUpdate,
    MatchUpdate,
    MembershipExpiring,
    SystemAlert,
    ChatMessage
}

public enum NotificationPriority { Low, Normal, High, Urgent }
```

### MemberNotificationPreference Entity
```csharp
public class MemberNotificationPreference : TenantEntity
{
    public Guid MemberId { get; private set; }
    public bool InAppEnabled { get; private set; }        // Default: true
    public bool EmailEnabled { get; private set; }        // Default: true
    public bool SmsEnabled { get; private set; }          // Default: false
    public bool PushEnabled { get; private set; }         // Default: true
    public bool MarketingEmailEnabled { get; private set; } // Default: false (opt-in)
    public List<NotificationType> MutedTypes { get; private set; } // Types to suppress
}
```

---

## 🔌 SignalR Hubs

### NotificationHub (Authorized)
```csharp
[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;  // From JWT claims
        var clubId = Context.User?.FindFirst("ClubId")?.Value;

        // Add to user-specific group
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");

        // Add to club group
        if (!string.IsNullOrEmpty(clubId))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"club-{clubId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Groups are automatically cleaned up on disconnect
        await base.OnDisconnectedAsync(exception);
    }

    // Client can mark notifications as read
    public async Task MarkAsRead(Guid notificationId)
    {
        var userId = Context.UserIdentifier;
        await _notificationService.MarkAsRead(notificationId, Guid.Parse(userId));
        await Clients.Caller.SendAsync("NotificationRead", notificationId);
    }
}
```

### MatchCentreHub (Public)
```csharp
public class MatchCentreHub : Hub
{
    // No [Authorize] — public access for live match updates

    public async Task SubscribeToMatch(Guid matchId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"match-{matchId}");
        await Clients.Caller.SendAsync("SubscribedToMatch", matchId);
    }

    public async Task UnsubscribeFromMatch(Guid matchId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"match-{matchId}");
    }

    // Server pushes these events to match subscribers:
    // "ScoreUpdate" — runs, wickets, overs
    // "MatchEvent" — boundaries, wickets, milestones
    // "StatusChange" — innings break, match completed
    // "Commentary" — ball-by-ball text commentary
}
```

### JWT Authentication via Query String
```csharp
// In Program.cs / Startup.cs
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // SignalR sends token via query string (WebSocket limitation)
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });
```

**Client connection example:**
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notifications", {
        accessTokenFactory: () => getAccessToken()
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .build();

connection.on("ReceiveNotification", (notification) => {
    showToast(notification.title, notification.body);
});
```

---

## 📡 INotificationService

### Interface
```csharp
public interface INotificationService
{
    Task SendToUser(Guid userId, Notification notification);
    Task SendToClub(Guid clubId, Notification notification);
    Task SendToMatch(Guid matchId, MatchUpdate update);
    Task SendToGroup(string groupName, Notification notification);
}
```

### Implementation
```csharp
public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly IHubContext<MatchCentreHub> _matchHub;
    private readonly IEmailProvider _emailProvider;
    private readonly ISmsProvider _smsProvider;
    private readonly IPushProvider _pushProvider;

    public async Task SendToUser(Guid userId, Notification notification)
    {
        var preferences = await GetPreferences(userId);

        // Channel 1: In-app (always, unless muted) — target: ≤2 seconds
        if (preferences.InAppEnabled && !preferences.MutedTypes.Contains(notification.Type))
        {
            await _notificationHub.Clients
                .Group($"user-{userId}")
                .SendAsync("ReceiveNotification", notification);
        }

        // Channel 2: Email — target: ≤30 seconds
        if (preferences.EmailEnabled && notification.Priority >= NotificationPriority.Normal)
        {
            await _emailProvider.SendAsync(BuildEmail(userId, notification));
        }

        // Channel 3: SMS — target: ≤30 seconds (high priority only)
        if (preferences.SmsEnabled && notification.Priority >= NotificationPriority.High)
        {
            await _smsProvider.SendAsync(BuildSms(userId, notification));
        }

        // Channel 4: Push notification — target: ≤30 seconds
        if (preferences.PushEnabled)
        {
            await _pushProvider.SendAsync(BuildPush(userId, notification));
        }

        // Persist notification record
        await _db.Notifications.AddAsync(notification);
        await _db.SaveChangesAsync();
    }

    public async Task SendToMatch(Guid matchId, MatchUpdate update)
    {
        // Public broadcast to all match subscribers
        await _matchHub.Clients
            .Group($"match-{matchId}")
            .SendAsync(update.EventType, update.Payload);
    }
}
```

---

## 🔄 Multi-Channel Delivery SLAs

```
┌─────────────────────────────────────────────────────────────┐
│                    Notification Triggered                     │
└──────────────────────────┬──────────────────────────────────┘
                           │
           ┌───────────────┼───────────────┬──────────────────┐
           ▼               ▼               ▼                  ▼
    ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌──────────────┐
    │   In-App    │ │    Email    │ │     SMS     │ │     Push     │
    │   ≤ 2s     │ │   ≤ 30s    │ │   ≤ 30s    │ │    ≤ 30s    │
    │  (SignalR)  │ │ (SendGrid) │ │  (Twilio)  │ │    (FCM)    │
    └─────────────┘ └─────────────┘ └─────────────┘ └──────────────┘
```

| Channel | Target SLA | Mechanism | Fallback |
|---------|-----------|-----------|----------|
| In-App | ≤ 2 seconds | SignalR WebSocket | Polling on reconnect |
| Email | ≤ 30 seconds | Async queue → provider | Retry 3× |
| SMS | ≤ 30 seconds | Async queue → provider | Retry 3× |
| Push | ≤ 30 seconds | Async queue → FCM/APNs | Retry 3× |

---

## 🔗 Webhook Service

### IWebhookService Interface
```csharp
public interface IWebhookService
{
    Task<WebhookResult> Deliver(Guid clubId, WebhookEvent webhookEvent);
    Task RegisterEndpoint(Guid clubId, WebhookEndpoint endpoint);
    Task RemoveEndpoint(Guid clubId, Guid endpointId);
}

public class WebhookEndpoint : TenantEntity
{
    public string Url { get; private set; }               // HTTPS endpoint
    public string? Secret { get; private set; }           // HMAC signing secret
    public List<string> SubscribedEvents { get; private set; } // Event types to receive
    public bool IsActive { get; private set; }
    public int ConsecutiveFailures { get; private set; }  // Disabled after 10 failures
}

public class WebhookEvent
{
    public string EventType { get; set; }                 // "payment.completed", "match.updated"
    public DateTime Timestamp { get; set; }
    public object Payload { get; set; }                   // Event-specific data
    public string Signature { get; set; }                 // HMAC-SHA256 of payload
}
```

### Webhook Delivery
```csharp
public class WebhookService : IWebhookService
{
    public async Task<WebhookResult> Deliver(Guid clubId, WebhookEvent webhookEvent)
    {
        var endpoints = await _db.WebhookEndpoints
            .Where(e => e.ClubId == clubId && e.IsActive
                && e.SubscribedEvents.Contains(webhookEvent.EventType))
            .ToListAsync();

        foreach (var endpoint in endpoints)
        {
            var payload = JsonSerializer.Serialize(webhookEvent);
            var signature = ComputeHmacSha256(payload, endpoint.Secret);

            var request = new HttpRequestMessage(HttpMethod.Post, endpoint.Url)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-Webhook-Signature", signature);
            request.Headers.Add("X-Webhook-Event", webhookEvent.EventType);

            try
            {
                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    await HandleFailure(endpoint);
                else
                    endpoint.ConsecutiveFailures = 0;
            }
            catch (Exception)
            {
                await HandleFailure(endpoint);
            }
        }

        return WebhookResult.Success();
    }

    private async Task HandleFailure(WebhookEndpoint endpoint)
    {
        endpoint.ConsecutiveFailures++;
        if (endpoint.ConsecutiveFailures >= 10)
            endpoint.IsActive = false; // Auto-disable after 10 consecutive failures
    }
}
```

---

## 🔄 Retry Logic (Exponential Backoff)

```csharp
public class NotificationRetryPolicy
{
    private const int MaxRetries = 3;
    // Exponential backoff: 5s, 25s (5²), 125s (5³)
    private static readonly TimeSpan[] Delays =
    {
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(25),
        TimeSpan.FromSeconds(125)
    };

    public async Task<DeliveryResult> DeliverWithRetry(
        Func<Task<DeliveryResult>> deliveryAction, string channel, Guid notificationId)
    {
        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            try
            {
                var result = await deliveryAction();
                if (result.Success)
                {
                    _logger.LogInformation(
                        "Notification {Id} delivered via {Channel} on attempt {Attempt}",
                        notificationId, channel, attempt + 1);
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Notification {Id} delivery failed via {Channel}, attempt {Attempt}",
                    notificationId, channel, attempt + 1);
            }

            if (attempt < MaxRetries)
                await Task.Delay(Delays[attempt]);
        }

        _logger.LogError("Notification {Id} permanently failed via {Channel}", notificationId, channel);
        return DeliveryResult.Failed($"Failed after {MaxRetries + 1} attempts");
    }
}
```

**Retry schedule:**
| Attempt | Delay After | Cumulative Wait |
|---------|-------------|-----------------|
| 1 (initial) | — | 0s |
| 2 (1st retry) | 5s | 5s |
| 3 (2nd retry) | 25s | 30s |
| 4 (3rd retry) | 125s | 155s |
| Give up | — | Mark as failed |

---

## 🌐 API Endpoints

| Method | Route | Permission | Purpose |
|--------|-------|-----------|---------|
| GET | /api/v1/notifications | ViewMembers | List user notifications |
| PUT | /api/v1/notifications/{id}/read | ViewMembers | Mark as read |
| PUT | /api/v1/notifications/read-all | ViewMembers | Mark all as read |
| GET | /api/v1/notifications/preferences | ViewMembers | Get channel preferences |
| PUT | /api/v1/notifications/preferences | ViewMembers | Update preferences |
| GET | /api/v1/webhooks | ManageMembers | List webhook endpoints |
| POST | /api/v1/webhooks | ManageMembers | Register endpoint |
| DELETE | /api/v1/webhooks/{id} | ManageMembers | Remove endpoint |
| POST | /api/v1/webhooks/test | ManageMembers | Send test webhook |

**SignalR Hub Endpoints:**
| Hub | Path | Auth | Purpose |
|-----|------|------|---------|
| NotificationHub | /hubs/notifications | JWT (query string) | Private notifications |
| MatchCentreHub | /hubs/match-centre | None (public) | Live match updates |

---

## 🧪 Testing Approach

### Property Tests
```
Property 36: Notification Respects Preferences
  For ANY notification sent to a member,
  delivery SHALL only occur on channels the member has enabled.

Property 37: Retry Terminates
  For ANY failed notification delivery,
  the retry mechanism SHALL terminate after exactly 3 retry attempts.

Property 38: Webhook Signature Validity
  For ANY webhook delivery, the X-Webhook-Signature header
  SHALL be a valid HMAC-SHA256 of the payload using the endpoint's secret.
```

### Unit Tests
- Send to user with in-app enabled → delivered via SignalR
- Send to user with email disabled → NOT sent via email
- Send to match group → all subscribers receive update
- Retry on transient failure → succeeds on 2nd attempt
- 4 consecutive failures → marked as permanently failed
- Webhook with 10 consecutive failures → endpoint auto-disabled
- JWT from query string → user authenticated correctly
- MatchCentreHub subscribe → added to match group
- Notification marked as read → IsRead = true, ReadAt set

---

## 🚀 How to Extend

### Adding notification batching/digest:
1. Queue low-priority notifications for 15 minutes
2. Batch into a single digest email/push
3. Reduce notification fatigue for active clubs

### Adding real-time typing indicators:
1. Add `TypingStarted`/`TypingStopped` methods to hub
2. Broadcast to relevant group (chat room, match commentary)
3. Client shows "X is typing..." indicator
