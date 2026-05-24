using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Sessions.Domain;
using TheLeague.Shared.Contracts.Services;

namespace TheLeague.Modules.Sessions.Infrastructure.Persistence;

public class SessionsDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<RecurringSchedule> RecurringSchedules => Set<RecurringSchedule>();
    public DbSet<SessionBooking> SessionBookings => Set<SessionBooking>();
    public DbSet<Waitlist> Waitlists => Set<Waitlist>();

    public SessionsDbContext(DbContextOptions<SessionsDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("sessions");

        var tenantId = _tenantService.CurrentTenantId ?? Guid.Empty;

        // Session
        builder.Entity<Session>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(100).IsRequired();
            e.Property(x => x.VenueName).HasMaxLength(200);
            e.Property(x => x.CancellationReason).HasMaxLength(500);
            e.Property(x => x.Fee).HasColumnType("decimal(18,2)");
            e.HasIndex(x => new { x.ClubId, x.StartTime });
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // RecurringSchedule
        builder.Entity<RecurringSchedule>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(100).IsRequired();
            e.Property(x => x.VenueName).HasMaxLength(200);
            e.Property(x => x.Fee).HasColumnType("decimal(18,2)");
            e.Property(x => x.StartTime).HasConversion(
                v => v.ToTimeSpan(),
                v => TimeOnly.FromTimeSpan(v));
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // SessionBooking
        builder.Entity<SessionBooking>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.SessionId, x.MemberId }).IsUnique();
            e.HasOne(x => x.Session)
                .WithMany(s => s.Bookings)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // Waitlist
        builder.Entity<Waitlist>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.HasOne(x => x.Session)
                .WithMany(s => s.WaitlistEntries)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });
    }
}
