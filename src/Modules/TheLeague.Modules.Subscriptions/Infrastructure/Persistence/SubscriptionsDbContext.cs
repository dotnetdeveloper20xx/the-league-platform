using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Subscriptions.Domain;

namespace TheLeague.Modules.Subscriptions.Infrastructure.Persistence;

public class SubscriptionsDbContext : DbContext
{
    public DbSet<SubscriptionTierConfig> TierConfigs => Set<SubscriptionTierConfig>();
    public DbSet<ClubSubscription> ClubSubscriptions => Set<ClubSubscription>();
    public DbSet<UsageRecord> UsageRecords => Set<UsageRecord>();
    public DbSet<AddOn> AddOns => Set<AddOn>();
    public DbSet<ClubAddOn> ClubAddOns => Set<ClubAddOn>();

    public SubscriptionsDbContext(DbContextOptions<SubscriptionsDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("subscriptions");

        builder.Entity<SubscriptionTierConfig>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Tier).IsUnique();
            e.Property(x => x.MonthlyPrice).HasColumnType("decimal(18,2)");
        });

        builder.Entity<ClubSubscription>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ClubId).IsUnique();
            e.Property(x => x.CurrentTier).HasConversion<string>();
            e.Property(x => x.ScheduledDowngradeTier).HasConversion<string>();
        });

        builder.Entity<UsageRecord>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ClubId);
            e.HasIndex(x => new { x.ClubId, x.PeriodStart, x.PeriodEnd });
        });

        builder.Entity<AddOn>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
        });

        builder.Entity<ClubAddOn>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ClubId);
            e.HasIndex(x => new { x.ClubId, x.AddOnId });
        });
    }
}
