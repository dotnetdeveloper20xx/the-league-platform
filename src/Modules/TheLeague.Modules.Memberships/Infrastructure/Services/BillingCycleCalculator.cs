using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Memberships.Infrastructure.Services;

public static class BillingCycleCalculator
{
    public static DateTime CalculateEndDate(DateTime startDate, BillingCycle billingCycle)
    {
        return billingCycle switch
        {
            BillingCycle.Weekly => startDate.AddDays(7),
            BillingCycle.Fortnightly => startDate.AddDays(14),
            BillingCycle.Monthly => startDate.AddMonths(1),
            BillingCycle.Quarterly => startDate.AddMonths(3),
            BillingCycle.Biannual => startDate.AddMonths(6),
            BillingCycle.Annual => startDate.AddYears(1),
            BillingCycle.Lifetime => DateTime.MaxValue,
            BillingCycle.OneTime => DateTime.MaxValue,
            BillingCycle.PayAsYouGo => DateTime.MaxValue,
            _ => throw new ArgumentOutOfRangeException(nameof(billingCycle), billingCycle, "Unsupported billing cycle.")
        };
    }
}
