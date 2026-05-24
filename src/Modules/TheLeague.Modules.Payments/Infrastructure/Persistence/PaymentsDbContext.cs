using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Payments.Domain;
using TheLeague.Shared.Contracts.Services;

namespace TheLeague.Modules.Payments.Infrastructure.Persistence;

public class PaymentsDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
    public DbSet<PaymentPlan> PaymentPlans => Set<PaymentPlan>();
    public DbSet<PaymentInstallment> PaymentInstallments => Set<PaymentInstallment>();
    public DbSet<MemberBalance> MemberBalances => Set<MemberBalance>();
    public DbSet<BalanceTransaction> BalanceTransactions => Set<BalanceTransaction>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<Fee> Fees => Set<Fee>();
    public DbSet<ChartOfAccount> ChartOfAccounts => Set<ChartOfAccount>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();

    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("payments");

        var tenantId = _tenantService.CurrentTenantId;

        // Payment
        builder.Entity<Payment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.PlatformFee).HasColumnType("decimal(18,2)");
            e.Property(x => x.ExternalTransactionId).HasMaxLength(500);
            e.Property(x => x.FailureReason).HasMaxLength(1000);
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasIndex(x => new { x.ClubId, x.PaymentDate });
            e.HasQueryFilter(x => tenantId == null || x.ClubId == tenantId);
        });

        // Invoice
        builder.Entity<Invoice>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.PaidAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Notes).HasMaxLength(2000);
            e.HasIndex(x => new { x.ClubId, x.InvoiceNumber }).IsUnique();
            e.HasMany(x => x.LineItems).WithOne().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => tenantId == null || x.ClubId == tenantId);
        });

        // InvoiceLineItem
        builder.Entity<InvoiceLineItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Description).HasMaxLength(500).IsRequired();
            e.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
            e.HasQueryFilter(x => tenantId == null || x.ClubId == tenantId);
        });

        // PaymentPlan
        builder.Entity<PaymentPlan>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Frequency).HasMaxLength(50);
            e.HasMany(x => x.Installments).WithOne().HasForeignKey(x => x.PaymentPlanId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => tenantId == null || x.ClubId == tenantId);
        });

        // PaymentInstallment
        builder.Entity<PaymentInstallment>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.HasQueryFilter(x => tenantId == null || x.ClubId == tenantId);
        });

        // MemberBalance
        builder.Entity<MemberBalance>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.CreditBalance).HasColumnType("decimal(18,2)");
            e.Property(x => x.OutstandingBalance).HasColumnType("decimal(18,2)");
            e.HasIndex(x => x.MemberId);
            e.HasQueryFilter(x => tenantId == null || x.ClubId == tenantId);
        });

        // BalanceTransaction
        builder.Entity<BalanceTransaction>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.ReferenceType).HasMaxLength(100);
            e.HasIndex(x => x.MemberId);
            e.HasQueryFilter(x => tenantId == null || x.ClubId == tenantId);
        });

        // Refund
        builder.Entity<Refund>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Reason).HasMaxLength(1000);
            e.HasQueryFilter(x => tenantId == null || x.ClubId == tenantId);
        });

        // Fee
        builder.Entity<Fee>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.BillingCycle).HasMaxLength(50);
            e.HasIndex(x => new { x.ClubId, x.Code }).IsUnique();
            e.HasQueryFilter(x => tenantId == null || x.ClubId == tenantId);
        });

        // ChartOfAccount
        builder.Entity<ChartOfAccount>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.AccountNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasQueryFilter(x => tenantId == null || x.ClubId == tenantId);
        });

        // JournalEntry
        builder.Entity<JournalEntry>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.ReferenceNumber).HasMaxLength(100);
            e.HasMany(x => x.Lines).WithOne().HasForeignKey(x => x.JournalEntryId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => tenantId == null || x.ClubId == tenantId);
        });

        // JournalEntryLine
        builder.Entity<JournalEntryLine>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.DebitAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.CreditAmount).HasColumnType("decimal(18,2)");
            e.Property(x => x.Description).HasMaxLength(500);
        });
    }
}
