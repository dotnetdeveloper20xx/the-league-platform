# 12 — Double-Entry Bookkeeping

## 📖 Feature Overview

The Bookkeeping module implements a double-entry accounting system for clubs. Every financial transaction (payment, refund, fee) automatically generates balanced journal entries. This provides clubs with a complete, auditable financial record that satisfies basic accounting principles.

### Key Capabilities
- Chart of Accounts with 5 account types (Asset, Liability, Equity, Revenue, Expense)
- Journal entries with debit/credit lines that must balance
- Automatic journal entry generation from payment events
- Fiscal year and period management
- Tax rate configuration and calculation
- Trial balance and financial statement support
- Immutable entries (corrections via reversing entries only)

---

## 🏗️ Architecture & Design Decisions

| Decision | Rationale |
|----------|-----------|
| Double-entry (not single-entry) | Industry standard; self-balancing; catches errors |
| Immutable journal entries | Audit compliance; corrections via reversing entries |
| Auto-generation from payments | No manual bookkeeping needed for standard transactions |
| Per-club Chart of Accounts | Clubs can customise account structure |
| Fiscal periods for reporting | Enables period-based P&L and balance sheets |
| Decimal(18,2) for amounts | Sufficient precision for GBP/USD; avoids floating-point |
| Event-driven entry creation | Payments module publishes → Bookkeeping subscribes |

---

## 📊 Data Model

### ChartOfAccount
```csharp
public class ChartOfAccount : TenantEntity
{
    public string AccountCode { get; private set; }       // "1000", "2100", "4000"
    public string Name { get; private set; }              // "Cash at Bank", "Accounts Receivable"
    public AccountType AccountType { get; private set; }
    public string? ParentAccountCode { get; private set; } // For sub-accounts
    public bool IsActive { get; private set; }
    public decimal CurrentBalance { get; private set; }   // Running balance
}

public enum AccountType
{
    Asset,       // What the club owns (Cash, Equipment, Receivables)
    Liability,   // What the club owes (Payables, Deferred Revenue)
    Equity,      // Owner's stake (Retained Earnings, Capital)
    Revenue,     // Income (Membership Fees, Session Fees, Event Tickets)
    Expense      // Costs (Facility Hire, Equipment, Platform Fees)
}
```

### Default Chart of Accounts (seeded per club)
```
Code  | Name                    | Type
------|-------------------------|----------
1000  | Cash at Bank            | Asset
1100  | Accounts Receivable     | Asset
1200  | Equipment Assets        | Asset
2000  | Accounts Payable        | Liability
2100  | Deferred Revenue        | Liability
2200  | Tax Payable             | Liability
3000  | Retained Earnings       | Equity
3100  | Club Capital            | Equity
4000  | Membership Revenue      | Revenue
4100  | Session Revenue         | Revenue
4200  | Event Revenue           | Revenue
4300  | Facility Hire Revenue   | Revenue
4400  | Equipment Hire Revenue  | Revenue
5000  | Platform Fees           | Expense
5100  | Facility Costs          | Expense
5200  | Equipment Maintenance   | Expense
5300  | Communication Costs     | Expense
```

---

## 📝 Journal Entries

### JournalEntry Entity
```csharp
public class JournalEntry : TenantEntity
{
    public string EntryNumber { get; private set; }       // "JE-2024-00001"
    public DateTime EntryDate { get; private set; }
    public string Description { get; private set; }       // "Membership payment - John Smith"
    public Guid? ReferenceId { get; private set; }        // PaymentId, RefundId, etc.
    public string? ReferenceType { get; private set; }    // "Payment", "Refund"
    public Guid? FiscalPeriodId { get; private set; }
    public bool IsReversing { get; private set; }         // True if this reverses another entry
    public Guid? ReversesEntryId { get; private set; }    // The entry being reversed

    public ICollection<JournalEntryLine> Lines { get; private set; }
}
```

### JournalEntryLine Entity
```csharp
public class JournalEntryLine : BaseEntity
{
    public Guid JournalEntryId { get; private set; }
    public string AccountCode { get; private set; }       // Links to ChartOfAccount
    public decimal DebitAmount { get; private set; }      // Either Debit or Credit, not both
    public decimal CreditAmount { get; private set; }
    public string? Description { get; private set; }      // Line-level description
}
```

### Balance Validation (The Golden Rule)
```csharp
public static JournalEntry Create(Guid clubId, string description,
    List<JournalEntryLine> lines, Guid? referenceId = null, string? referenceType = null)
{
    // RULE: Sum of debits MUST equal sum of credits
    var totalDebits = lines.Sum(l => l.DebitAmount);
    var totalCredits = lines.Sum(l => l.CreditAmount);

    if (totalDebits != totalCredits)
        throw new InvalidOperationException(
            $"Journal entry is unbalanced. Debits: {totalDebits}, Credits: {totalCredits}");

    // RULE: Must have at least 2 lines
    if (lines.Count < 2)
        throw new InvalidOperationException("Journal entry must have at least 2 lines.");

    // RULE: Each line must have either debit OR credit, not both
    if (lines.Any(l => l.DebitAmount > 0 && l.CreditAmount > 0))
        throw new InvalidOperationException("A line cannot have both debit and credit amounts.");

    // RULE: No negative amounts
    if (lines.Any(l => l.DebitAmount < 0 || l.CreditAmount < 0))
        throw new InvalidOperationException("Amounts cannot be negative.");

    return new JournalEntry { /* ... */ };
}
```

---

## 📐 The Accounting Equation

```
Assets = Liabilities + Equity + (Revenue - Expenses)
```

**Normal balances by account type:**
| Account Type | Normal Balance | Increases With | Decreases With |
|-------------|---------------|----------------|----------------|
| Asset | Debit | Debit | Credit |
| Liability | Credit | Credit | Debit |
| Equity | Credit | Credit | Debit |
| Revenue | Credit | Credit | Debit |
| Expense | Debit | Debit | Credit |

---

## 🔄 Auto-Generated Journal Entries

### When a Membership Payment is Received (£50)
```
Debit:  1000 Cash at Bank         £50.00
Credit: 4000 Membership Revenue   £50.00
Description: "Membership payment - John Smith (Adult Annual)"
```

### When Platform Fee is Deducted (£0.75)
```
Debit:  5000 Platform Fees        £0.75
Credit: 1000 Cash at Bank         £0.75
Description: "Platform fee for payment PAY-2024-00123"
```

### When a Refund is Processed (£50)
```
Debit:  4000 Membership Revenue   £50.00
Credit: 1000 Cash at Bank         £50.00
Description: "Refund - John Smith membership cancellation"
```

### When an Invoice is Issued (before payment)
```
Debit:  1100 Accounts Receivable  £50.00
Credit: 2100 Deferred Revenue     £50.00
Description: "Invoice INV-2024-00045 issued to Jane Doe"
```

### When Invoice Payment is Received
```
Debit:  1000 Cash at Bank         £50.00
Credit: 1100 Accounts Receivable  £50.00
Description: "Payment received for INV-2024-00045"

Debit:  2100 Deferred Revenue     £50.00
Credit: 4000 Membership Revenue   £50.00
Description: "Revenue recognised for INV-2024-00045"
```

### Event Handler (subscribes to PaymentCompletedEvent)
```csharp
public class PaymentCompletedBookkeepingHandler
    : IIntegrationEventHandler<PaymentCompletedEvent>
{
    public async Task Handle(PaymentCompletedEvent @event)
    {
        var lines = new List<JournalEntryLine>
        {
            new(accountCode: "1000", debit: @event.Amount, credit: 0),
            new(accountCode: GetRevenueAccount(@event), debit: 0, credit: @event.Amount)
        };

        var entry = JournalEntry.Create(
            @event.ClubId,
            $"Payment received - {@event.PaymentMethod}",
            lines,
            referenceId: @event.PaymentId,
            referenceType: "Payment");

        await _repository.AddAsync(entry);
    }
}
```

---

## 📅 Fiscal Years & Periods

### FiscalYear Entity
```csharp
public class FiscalYear : TenantEntity
{
    public string Name { get; private set; }          // "2024/25"
    public DateTime StartDate { get; private set; }   // e.g., 1 April 2024
    public DateTime EndDate { get; private set; }     // e.g., 31 March 2025
    public bool IsClosed { get; private set; }        // No new entries when closed
    public ICollection<FiscalPeriod> Periods { get; private set; }
}

public class FiscalPeriod : TenantEntity
{
    public Guid FiscalYearId { get; private set; }
    public string Name { get; private set; }          // "April 2024", "Q1 2024/25"
    public int PeriodNumber { get; private set; }     // 1-12 (monthly) or 1-4 (quarterly)
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsClosed { get; private set; }
}
```

**Period rules:**
- Fiscal year can be any 12-month period (not necessarily Jan-Dec)
- Periods cannot overlap within a fiscal year
- Closed periods reject new journal entries
- Year-end closing creates a summary entry to Retained Earnings

---

## 💰 Tax Rates

```csharp
public class TaxRate : TenantEntity
{
    public string Name { get; private set; }          // "Standard VAT", "Reduced Rate"
    public decimal Rate { get; private set; }         // 0.20 = 20%
    public bool IsDefault { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }
}
```

**Tax calculation in invoicing:**
```csharp
public decimal CalculateTax(decimal netAmount, TaxRate taxRate)
{
    return Math.Round(netAmount * taxRate.Rate, 2);
}

// Example: £100 net + 20% VAT = £120 gross
// Journal entry for tax:
// Debit:  1100 Accounts Receivable  £120.00
// Credit: 4000 Revenue              £100.00
// Credit: 2200 Tax Payable          £20.00
```

---

## 📊 Trial Balance

```csharp
public class TrialBalanceReport
{
    public DateTime AsOfDate { get; set; }
    public List<TrialBalanceLine> Lines { get; set; }
    public decimal TotalDebits => Lines.Sum(l => l.DebitBalance);
    public decimal TotalCredits => Lines.Sum(l => l.CreditBalance);
    public bool IsBalanced => TotalDebits == TotalCredits;
}

public class TrialBalanceLine
{
    public string AccountCode { get; set; }
    public string AccountName { get; set; }
    public AccountType AccountType { get; set; }
    public decimal DebitBalance { get; set; }
    public decimal CreditBalance { get; set; }
}
```

**A balanced trial balance confirms:**
- All journal entries were balanced when created
- No data corruption has occurred
- The accounting equation holds

---

## 🌐 API Endpoints

| Method | Route | Permission | Purpose |
|--------|-------|-----------|---------|
| GET | /api/v1/bookkeeping/accounts | ManageMembers | List chart of accounts |
| POST | /api/v1/bookkeeping/accounts | ManageMembers | Create account |
| GET | /api/v1/bookkeeping/journal-entries | ManageMembers | List journal entries |
| POST | /api/v1/bookkeeping/journal-entries | ManageMembers | Create manual entry |
| POST | /api/v1/bookkeeping/journal-entries/{id}/reverse | ManageMembers | Reverse entry |
| GET | /api/v1/bookkeeping/trial-balance | ManageMembers | Generate trial balance |
| GET | /api/v1/bookkeeping/fiscal-years | ManageMembers | List fiscal years |
| POST | /api/v1/bookkeeping/fiscal-years | ManageMembers | Create fiscal year |
| PUT | /api/v1/bookkeeping/fiscal-periods/{id}/close | ManageMembers | Close period |

---

## 🧪 Testing Approach

### Property Tests
```
Property 15: Journal Entry Balance
  For ANY journal entry created in the system,
  the sum of all debit amounts SHALL equal the sum of all credit amounts.

Property 16: Trial Balance
  For ANY point in time, the trial balance total debits
  SHALL equal total credits (proving the accounting equation holds).

Property 17: Account Balance Direction
  For ANY Asset or Expense account, the normal balance SHALL be debit.
  For ANY Liability, Equity, or Revenue account, the normal balance SHALL be credit.
```

### Unit Tests
- Create entry with unbalanced lines → throws InvalidOperationException
- Create entry with single line → throws (minimum 2 lines)
- Create entry with negative amount → throws
- Payment event → generates correct debit/credit pair
- Refund event → generates reversing entry
- Close fiscal period → reject new entries in that period
- Trial balance after 10 transactions → debits equal credits

---

## 🚀 How to Extend

### Adding profit & loss report:
1. Query all Revenue and Expense accounts for a fiscal period
2. Revenue total - Expense total = Net Profit/Loss
3. Present as a structured report with account breakdowns

### Adding balance sheet:
1. Query all Asset, Liability, and Equity accounts as of a date
2. Verify Assets = Liabilities + Equity
3. Include current-period P&L in Equity section
