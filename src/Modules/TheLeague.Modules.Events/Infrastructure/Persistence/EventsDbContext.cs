using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Events.Domain;
using TheLeague.Shared.Contracts.Services;

namespace TheLeague.Modules.Events.Infrastructure.Persistence;

public class EventsDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventSeries> EventSeries => Set<EventSeries>();
    public DbSet<EventSession> EventSessions => Set<EventSession>();
    public DbSet<EventTicket> EventTickets => Set<EventTicket>();
    public DbSet<EventRSVP> EventRSVPs => Set<EventRSVP>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();

    public EventsDbContext(DbContextOptions<EventsDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("events");

        var tenantId = _tenantService.CurrentTenantId ?? Guid.Empty;

        // Event configuration
        builder.Entity<Event>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(4000);
            e.Property(x => x.EventType).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.VenueName).HasMaxLength(200);
            e.Property(x => x.StandardPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.MemberPrice).HasColumnType("decimal(18,2)");

            e.HasIndex(x => new { x.ClubId, x.StartDateTime });
            e.HasIndex(x => new { x.ClubId, x.Status });

            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // EventSeries configuration
        builder.Entity<EventSeries>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.EventType).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.RecurrencePattern).HasMaxLength(2000);

            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // EventSession configuration
        builder.Entity<EventSession>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.VenueName).HasMaxLength(200);

            e.HasIndex(x => new { x.EventId, x.SessionOrder });

            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // EventTicket configuration
        builder.Entity<EventTicket>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TicketNumber).HasMaxLength(50).IsRequired();
            e.Property(x => x.QRCodeData).HasMaxLength(2000).IsRequired();
            e.Property(x => x.PricePaid).HasColumnType("decimal(18,2)");

            e.HasIndex(x => x.TicketNumber).IsUnique();
            e.HasIndex(x => new { x.EventId, x.MemberId });

            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // EventRSVP configuration
        builder.Entity<EventRSVP>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Response).HasConversion<string>().HasMaxLength(20);

            e.HasIndex(x => new { x.EventId, x.MemberId }).IsUnique();

            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // EventRegistration configuration
        builder.Entity<EventRegistration>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.RegistrationType).HasMaxLength(20).IsRequired();

            e.HasIndex(x => new { x.EventId, x.MemberId });

            e.HasQueryFilter(x => x.ClubId == tenantId);
        });
    }
}
