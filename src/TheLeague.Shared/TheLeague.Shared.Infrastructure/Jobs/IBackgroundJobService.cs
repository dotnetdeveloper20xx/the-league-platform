namespace TheLeague.Shared.Infrastructure.Jobs;

public interface IBackgroundJobService
{
    Task ScheduleMembershipRenewalCheckAsync(CancellationToken ct = default);
    Task SchedulePaymentRemindersAsync(CancellationToken ct = default);
    Task ScheduleOverdueInvoiceCheckAsync(CancellationToken ct = default);
    Task ScheduleEquipmentLoanOverdueCheckAsync(CancellationToken ct = default);
    Task ScheduleTrialExpiryCheckAsync(CancellationToken ct = default);
    Task ScheduleDataRetentionCleanupAsync(CancellationToken ct = default);
}
