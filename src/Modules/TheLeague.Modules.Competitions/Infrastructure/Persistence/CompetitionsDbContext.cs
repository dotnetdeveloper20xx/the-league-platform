using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Competitions.Domain;
using TheLeague.Shared.Contracts.Services;

namespace TheLeague.Modules.Competitions.Infrastructure.Persistence;

public class CompetitionsDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public DbSet<Season> Seasons => Set<Season>();
    public DbSet<Competition> Competitions => Set<Competition>();
    public DbSet<CompetitionTeam> CompetitionTeams => Set<CompetitionTeam>();
    public DbSet<CompetitionParticipant> CompetitionParticipants => Set<CompetitionParticipant>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchEvent> MatchEvents => Set<MatchEvent>();
    public DbSet<MatchLineup> MatchLineups => Set<MatchLineup>();
    public DbSet<CompetitionStanding> CompetitionStandings => Set<CompetitionStanding>();

    public CompetitionsDbContext(DbContextOptions<CompetitionsDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("competitions");

        var tenantId = _tenantService.CurrentTenantId ?? Guid.Empty;

        // Season
        builder.Entity<Season>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // Competition
        builder.Entity<Competition>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.DefaultWalkoverScore).HasMaxLength(50);
            e.HasIndex(x => new { x.ClubId, x.SeasonId });
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // CompetitionTeam
        builder.Entity<CompetitionTeam>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TeamName).HasMaxLength(100).IsRequired();
            e.Property(x => x.HomeVenueName).HasMaxLength(200);
            e.Property(x => x.TeamColor).HasMaxLength(20);
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // CompetitionParticipant
        builder.Entity<CompetitionParticipant>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.TeamId, x.MemberId }).IsUnique();
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // Match
        builder.Entity<Match>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.VenueName).HasMaxLength(200);
            e.Property(x => x.HomeScore).HasMaxLength(500);
            e.Property(x => x.AwayScore).HasMaxLength(500);
            e.HasIndex(x => new { x.CompetitionId, x.ScheduledDateTime });
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // MatchEvent
        builder.Entity<MatchEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.EventType).HasMaxLength(50).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.HasIndex(x => new { x.MatchId, x.Timestamp });
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // MatchLineup
        builder.Entity<MatchLineup>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Position).HasMaxLength(50);
            e.HasIndex(x => new { x.MatchId, x.TeamId, x.PlayerId }).IsUnique();
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // CompetitionStanding
        builder.Entity<CompetitionStanding>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Form).HasMaxLength(10);
            e.HasIndex(x => new { x.CompetitionId, x.TeamId }).IsUnique();
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });
    }
}
