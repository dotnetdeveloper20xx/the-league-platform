using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Analytics.Domain;
using TheLeague.Shared.Contracts.Services;

namespace TheLeague.Modules.Analytics.Infrastructure.Persistence;

public class AnalyticsDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public DbSet<ClubAnalyticsSnapshot> Snapshots => Set<ClubAnalyticsSnapshot>();
    public DbSet<MemberEngagement> MemberEngagements => Set<MemberEngagement>();
    public DbSet<ChurnPrediction> ChurnPredictions => Set<ChurnPrediction>();

    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("analytics");

        // Tenant query filters
        builder.Entity<ClubAnalyticsSnapshot>().HasQueryFilter(e => e.ClubId == _tenantService.CurrentTenantId);
        builder.Entity<MemberEngagement>().HasQueryFilter(e => e.ClubId == _tenantService.CurrentTenantId);
        builder.Entity<ChurnPrediction>().HasQueryFilter(e => e.ClubId == _tenantService.CurrentTenantId);

        // ClubAnalyticsSnapshot configuration
        builder.Entity<ClubAnalyticsSnapshot>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.MemberGrowthRate).HasPrecision(18, 4);
            e.Property(x => x.PaymentCollectionRate).HasPrecision(18, 4);
            e.Property(x => x.SessionAttendanceRate).HasPrecision(18, 4);
            e.Property(x => x.EventParticipationRate).HasPrecision(18, 4);
            e.Property(x => x.TotalRevenue).HasPrecision(18, 2);
            e.HasIndex(x => new { x.ClubId, x.SnapshotDate });
        });

        // MemberEngagement configuration
        builder.Entity<MemberEngagement>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PaymentTimelinessDays).HasPrecision(18, 2);
            e.HasIndex(x => new { x.MemberId, x.Month });
            e.HasIndex(x => new { x.ClubId, x.Month });
        });

        // ChurnPrediction configuration
        builder.Entity<ChurnPrediction>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.RiskFactors).HasMaxLength(1000);
            e.Property(x => x.AttendanceDropPercent).HasPrecision(18, 2);
            e.Property(x => x.LoginDropPercent).HasPrecision(18, 2);
            e.HasIndex(x => new { x.ClubId, x.MemberId, x.PredictionDate });
            e.HasIndex(x => new { x.ClubId, x.IsAtRisk });
        });
    }
}
