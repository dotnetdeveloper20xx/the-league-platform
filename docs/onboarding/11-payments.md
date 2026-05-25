# 11 — Payment Processing & Stripe Connect

## 📖 Feature Overview

The Payments module handles all financial transactions across the platform — membership fees, session bookings, event tickets, facility hire, and equipment deposits. It uses a provider-based architecture with Stripe Connect as the production gateway and a mock provider for development/testing.

### Key Capabilities
- Multi-provider payment architecture (Stripe Connect + Mock)
- Platform fee calculation (1-2% with minimum £0.30)
- Invoice lifecycle management (Draft → Paid → Voided)
- Payment plans with installment scheduling
- Member balance ledger (credits, debits, outstanding balance)
- Refund processing (full and partial)
- Integration events for cross-module communication
- Payment failure handling with retry logic

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| Provider abstraction (`IPaymentProvider`) | Swap Stripe for mock in tests; future-proof for other gateways |
| Stripe Connect (platform model) | Each club is a Connected Account; platform takes a fee per transaction |
| Platform fee as percentage + minimum | Covers Stripe's base cost; scales with transaction size |
| Invoice as first-class entity | Audit trail; supports partial payments and payment plans |
| Ledger-based balance tracking | Double-entry style credits/debits; always reconcilable |
| Events for cross-module sync | Payments module publishes; Memberships/Bookings consume |
| Idempotency keys on all charges | Prevents duplicate charges on network retries |

---

## 📊 Data Model

### Payment Entity
```csharp
public class Payment : TenantEntity
{
    public Guid MemberId { get; private set; }
    public Guid? InvoiceId { get; private set; }
    public decimal Amount { get; private set; }              // Gross amount charged
    public decimal PlatformFee { get; private set; }         // Fee retained by platform
    public decimal NetAmount { get; private set; }           // Amount transferred to club
    public string Currency { get; private set; }             // ISO 4217: "GBP", "USD"
    public PaymentMethod PaymentMethod { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? ExternalTransactionId { get; private set; } // Stripe charge ID
    public string? FailureReason { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public string? IdempotencyKey { get; private set; }
}
```

### PaymentMethod Enum
```csharp
public enum PaymentMethod
{
    Card,           // Credit/debit card via Stripe
    DirectDebit,    // UK Direct Debit (GoCardless future)
    BankTransfer,   // Manual bank transfer
    Cash,           // In-person cash payment
    Credit          // Applied from member credit balance
}
```

### PaymentStatus Enum
```csharp
public enum PaymentStatus
{
    Pending,        // Payment initiated, awaiting confirmation
    Processing,     // Sent to provider, awaiting response
    Completed,      // Successfully charged
    Failed,         // Charge attempt failed
    Refunded,       // Fully refunded
    PartialRefund,  // Partially refunded
    Cancelled       // Cancelled before processing
}
```

---

## 🔌 Provider Architecture

### IPaymentProvider Interface
```csharp
public interface IPaymentProvider
{
    Task<PaymentResult> ChargeAsync(ChargeRequest request);
    Task<RefundResult> RefundAsync(RefundRequest request);
    Task<PaymentResult> GetStatusAsync(string externalTransactionId);
    Task<string> CreateConnectedAccountAsync(ConnectedAccountRequest request);
    Task<AccountStatus> GetAccountStatusAsync(string connectedAccountId);
}
```

### MockPaymentProvider (Development/Testing)
```csharp
public class MockPaymentProvider : IPaymentProvider
{
    public Task<PaymentResult> ChargeAsync(ChargeRequest request)
    {
        // Simulate success for amounts < £9999
        // Simulate failure for amounts >= £9999 (test failure paths)
        if (request.Amount >= 9999m)
            return Task.FromResult(PaymentResult.Failed("Mock: Amount too high"));

        return Task.FromResult(PaymentResult.Success(
            transactionId: $"mock_ch_{Guid.NewGuid():N}",
            amount: request.Amount));
    }

    public Task<RefundResult> RefundAsync(RefundRequest request)
    {
        return Task.FromResult(RefundResult.Success(
            refundId: $"mock_re_{Guid.NewGuid():N}"));
    }
}
```

### PaymentProviderFactory
```csharp
public class PaymentProviderFactory : IPaymentProviderFactory
{
    private readonly IConfiguration _config;

    public IPaymentProvider Create()
    {
        var provider = _config["Payments:Provider"]; // "Stripe" or "Mock"
        return provider switch
        {
            "Stripe" => new StripePaymentProvider(_config),
            "Mock" => new MockPaymentProvider(),
            _ => throw new InvalidOperationException($"Unknown provider: {provider}")
        };
    }
}
```

---

## 💷 Stripe Connect & Platform Fees

### How Stripe Connect Works
```
┌──────────┐     Charge      ┌──────────────┐     Transfer     ┌──────────────┐
│  Member  │────────────────▶│  The League  │────────────────▶│  Club Stripe │
│ (Payer)  │                 │  (Platform)  │  (minus fee)    │  (Connected) │
└──────────┘                 └──────────────┘                 └──────────────┘
                                    │
                                    ▼
                             Platform Fee
                             retained by
                             The League
```

### PlatformFeeCalculator
```csharp
public class PlatformFeeCalculator
{
    private const decimal MinimumFee = 0.30m;       // £0.30 minimum
    private const decimal DefaultRate = 0.015m;     // 1.5% default

    public decimal Calculate(decimal amount, decimal? overrideRate = null)
    {
        var rate = overrideRate ?? DefaultRate;

        // Rate must be between 1% and 2%
        rate = Math.Clamp(rate, 0.01m, 0.02m);

        var calculatedFee = Math.Round(amount * rate, 2);
        return Math.Max(calculatedFee, MinimumFee);
    }
}
```

**Fee examples:**
| Transaction | Rate | Calculated | Applied (with min) |
|-------------|------|-----------|-------------------|
| £10.00 | 1.5% | £0.15 | £0.30 (minimum) |
| £25.00 | 1.5% | £0.38 | £0.38 |
| £100.00 | 1.5% | £1.50 | £1.50 |
| £500.00 | 2.0% | £10.00 | £10.00 |

---

## 🧾 Invoice Lifecycle

### Invoice Entity
```csharp
public class Invoice : TenantEntity
{
    public Guid MemberId { get; private set; }
    public string InvoiceNumber { get; private set; }     // "INV-2024-00001"
    public decimal TotalAmount { get; private set; }
    public decimal PaidAmount { get; private set; }
    public decimal OutstandingAmount => TotalAmount - PaidAmount;
    public InvoiceStatus Status { get; private set; }
    public DateTime IssuedDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public ICollection<InvoiceLineItem> LineItems { get; private set; }
    public ICollection<Payment> Payments { get; private set; }
}
```

### Invoice Status State Machine
```
┌───────┐   Send()   ┌────────┐   View()   ┌────────┐
│ Draft │───────────▶│  Sent  │───────────▶│ Viewed │
└───────┘            └───┬────┘            └───┬────┘
                         │                     │
                         │    PartialPay()      │  PartialPay()
                         ▼                     ▼
                    ┌──────────────┐     ┌──────────────┐
                    │ PartiallyPaid│◀────│ PartiallyPaid│
                    └──────┬───────┘     └──────────────┘
                           │
                           │ FullPay()
                           ▼
                      ┌────────┐
                      │  Paid  │ (terminal)
                      └────────┘

  Any non-terminal ──▶ Overdue (when DueDate passes)
  Any non-terminal ──▶ Voided  (manual cancellation)
```

### InvoiceStatus Enum
```csharp
public enum InvoiceStatus
{
    Draft,          // Being prepared, not yet sent
    Sent,           // Delivered to member
    Viewed,         // Member has opened/viewed
    PartiallyPaid,  // Some payment received
    Paid,           // Fully paid (terminal)
    Overdue,        // Past due date, unpaid
    Voided          // Cancelled/written off (terminal)
}
```

---

## 📅 Payment Plans & Installments

```csharp
public class PaymentPlan : TenantEntity
{
    public Guid MemberId { get; private set; }
    public Guid InvoiceId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public int NumberOfInstallments { get; private set; }  // 2-12
    public decimal InstallmentAmount { get; private set; } // TotalAmount / NumberOfInstallments
    public PaymentPlanStatus Status { get; private set; }  // Active, Completed, Defaulted

    public ICollection<Installment> Installments { get; private set; }
}

public class Installment : TenantEntity
{
    public Guid PaymentPlanId { get; private set; }
    public int SequenceNumber { get; private set; }       // 1, 2, 3...
    public decimal Amount { get; private set; }
    public DateTime DueDate { get; private set; }
    public InstallmentStatus Status { get; private set; } // Pending, Paid, Overdue, Failed
    public Guid? PaymentId { get; private set; }          // Links to Payment when paid
}
```

**Installment calculation:**
```csharp
public static PaymentPlan Create(Guid memberId, Guid invoiceId,
    decimal totalAmount, int installments)
{
    if (installments < 2 || installments > 12)
        throw new ArgumentException("Installments must be between 2 and 12.");

    var installmentAmount = Math.Round(totalAmount / installments, 2);
    // Last installment absorbs rounding difference
    var lastInstallment = totalAmount - (installmentAmount * (installments - 1));

    // Generate installments with monthly due dates
    // ...
}
```

---

## 💰 Member Balance Ledger

```csharp
public class MemberBalanceLedger : TenantEntity
{
    public Guid MemberId { get; private set; }
    public LedgerEntryType EntryType { get; private set; }
    public decimal Amount { get; private set; }           // Always positive
    public decimal RunningBalance { get; private set; }   // Balance after this entry
    public string Description { get; private set; }
    public Guid? ReferenceId { get; private set; }        // PaymentId, RefundId, etc.
    public string? ReferenceType { get; private set; }    // "Payment", "Refund", "Credit"
}

public enum LedgerEntryType
{
    Credit,     // Money added (refund, manual credit, overpayment)
    Debit,      // Money owed (invoice, fee, charge)
    Payment     // Money paid (reduces outstanding)
}
```

**Balance calculation:**
```csharp
// Outstanding = sum of Debits - sum of Payments - sum of Credits
public decimal GetOutstandingBalance(Guid memberId)
{
    var entries = _db.MemberBalanceLedger.Where(l => l.MemberId == memberId);
    var debits = entries.Where(e => e.EntryType == LedgerEntryType.Debit).Sum(e => e.Amount);
    var payments = entries.Where(e => e.EntryType == LedgerEntryType.Payment).Sum(e => e.Amount);
    var credits = entries.Where(e => e.EntryType == LedgerEntryType.Credit).Sum(e => e.Amount);
    return debits - payments - credits;
}
```

---

## 🔄 Refund Processing

```csharp
public class Refund : TenantEntity
{
    public Guid PaymentId { get; private set; }
    public decimal Amount { get; private set; }
    public string Reason { get; private set; }
    public RefundStatus Status { get; private set; }      // Pending, Processed, Failed
    public string? ExternalRefundId { get; private set; } // Stripe refund ID
    public Guid ProcessedBy { get; private set; }         // Admin who approved
}
```

**Refund rules:**
- Full refund: Amount equals original payment amount
- Partial refund: Amount less than original; multiple partials allowed up to original amount
- Only `Completed` payments can be refunded
- Refund creates a `Credit` entry in the member's balance ledger
- Platform fee is NOT refunded (Stripe's policy)

---

## 📡 Integration Events

```csharp
// Published when payment succeeds
public record PaymentCompletedEvent(
    Guid PaymentId,
    Guid ClubId,
    Guid MemberId,
    decimal Amount,
    string? InvoiceId,
    string PaymentMethod,
    DateTime CompletedAt) : IIntegrationEvent;

// Published when payment fails
public record PaymentFailedEvent(
    Guid PaymentId,
    Guid ClubId,
    Guid MemberId,
    decimal Amount,
    string FailureReason,
    int AttemptNumber,
    DateTime FailedAt) : IIntegrationEvent;
```

**Consumers:**
- `MembershipModule` → listens for `PaymentCompletedEvent` to activate memberships
- `BookingModule` → listens for `PaymentCompletedEvent` to confirm bookings
- `CommunicationsModule` → listens for `PaymentFailedEvent` to send payment reminders

---

## 🔥 Payment Failure Handling

```csharp
public class PaymentRetryPolicy
{
    private static readonly int[] RetryDelaysHours = { 24, 72, 168 }; // 1 day, 3 days, 7 days

    public bool ShouldRetry(int attemptNumber) => attemptNumber <= 3;

    public DateTime GetNextRetryDate(int attemptNumber)
    {
        var delayHours = RetryDelaysHours[attemptNumber - 1];
        return DateTime.UtcNow.AddHours(delayHours);
    }
}
```

**Failure flow:**
```
Attempt 1 fails → wait 24h → Attempt 2
Attempt 2 fails → wait 72h → Attempt 3
Attempt 3 fails → wait 168h → Mark as Failed
                              → Publish PaymentFailedEvent (final)
                              → Membership set to PendingPayment
```

---

## 🌐 API Endpoints

| Method | Route | Permission | Purpose |
|--------|-------|-----------|---------|
| POST | /api/v1/payments/charge | ManageMembers | Initiate payment |
| GET | /api/v1/payments/{id} | ViewMembers | Get payment details |
| GET | /api/v1/payments/member/{memberId} | ViewMembers | Member payment history |
| POST | /api/v1/payments/{id}/refund | ManageMembers | Process refund |
| GET | /api/v1/invoices | ViewMembers | List invoices |
| POST | /api/v1/invoices | ManageMembers | Create invoice |
| PUT | /api/v1/invoices/{id}/send | ManageMembers | Send invoice |
| PUT | /api/v1/invoices/{id}/void | ManageMembers | Void invoice |
| POST | /api/v1/payment-plans | ManageMembers | Create payment plan |
| GET | /api/v1/members/{id}/balance | ViewMembers | Get member balance |

---

## 🧪 Testing Approach

### Property Tests
```
Property 12: Platform Fee Bounds
  For ANY transaction amount > 0,
  the platform fee SHALL be >= £0.30 AND <= 2% of the amount.

Property 13: Ledger Balance Consistency
  For ANY member, the running balance of the last ledger entry
  SHALL equal sum(Credits) - sum(Debits) + sum(Payments).

Property 14: Installment Sum Equals Total
  For ANY payment plan, the sum of all installment amounts
  SHALL equal the plan's TotalAmount exactly.
```

### Unit Tests
- Charge £10 → platform fee = £0.30 (minimum applies)
- Charge £100 at 1.5% → platform fee = £1.50
- Refund more than original amount → throws
- Create 12-installment plan for £100 → installments sum to £100
- Mock provider: charge £9999 → returns failure
- Payment retry after 3 failures → marked as Failed

---

## 🚀 How to Extend

### Adding a new payment provider (e.g., GoCardless):
1. Implement `IPaymentProvider` interface
2. Register in `PaymentProviderFactory`
3. Add configuration section in `appsettings.json`
4. Map provider-specific status codes to `PaymentStatus` enum

### Adding subscription billing:
1. Create `RecurringPayment` entity with schedule
2. Background job to process due recurring payments
3. Link to `MembershipType.BillingCycle` for timing
