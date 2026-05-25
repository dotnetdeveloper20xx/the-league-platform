using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Facilities.Domain;
using TheLeague.Shared.Contracts.Services;

namespace TheLeague.Modules.Facilities.Infrastructure.Persistence;

public class FacilitiesDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public DbSet<Facility> Facilities => Set<Facility>();
    public DbSet<FacilityBooking> FacilityBookings => Set<FacilityBooking>();
    public DbSet<FacilityAvailability> FacilityAvailabilities => Set<FacilityAvailability>();
    public DbSet<FacilityPricing> FacilityPricings => Set<FacilityPricing>();
    public DbSet<FacilityMaintenance> FacilityMaintenances => Set<FacilityMaintenance>();
    public DbSet<FacilityBlockout> FacilityBlockouts => Set<FacilityBlockout>();

    public FacilitiesDbContext(DbContextOptions<FacilitiesDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("facilities");

        var tenantId = _tenantService.CurrentTenantId ?? Guid.Empty;

        // Facility
        builder.Entity<Facility>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // FacilityBooking
        builder.Entity<FacilityBooking>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PricePaid).HasColumnType("decimal(18,2)");
            e.Property(x => x.BookingReference).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.BookingReference).IsUnique();
            e.HasIndex(x => new { x.FacilityId, x.BookingDate, x.StartTime });
            e.Property(x => x.StartTime).HasConversion(
                v => v.ToTimeSpan(),
                v => TimeOnly.FromTimeSpan(v));
            e.Property(x => x.EndTime).HasConversion(
                v => v.ToTimeSpan(),
                v => TimeOnly.FromTimeSpan(v));
            e.Property(x => x.BookingDate).HasConversion(
                v => v.ToDateTime(TimeOnly.MinValue),
                v => DateOnly.FromDateTime(v));
            e.HasOne(x => x.Facility)
                .WithMany(f => f.Bookings)
                .HasForeignKey(x => x.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // FacilityAvailability
        builder.Entity<FacilityAvailability>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.OpenTime).HasConversion(
                v => v.ToTimeSpan(),
                v => TimeOnly.FromTimeSpan(v));
            e.Property(x => x.CloseTime).HasConversion(
                v => v.ToTimeSpan(),
                v => TimeOnly.FromTimeSpan(v));
            e.HasOne(x => x.Facility)
                .WithMany(f => f.Availabilities)
                .HasForeignKey(x => x.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // FacilityPricing
        builder.Entity<FacilityPricing>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.MemberRate).HasColumnType("decimal(18,2)");
            e.Property(x => x.NonMemberRate).HasColumnType("decimal(18,2)");
            e.Property(x => x.PeakStartTime).HasConversion(
                v => v.HasValue ? v.Value.ToTimeSpan() : (TimeSpan?)null,
                v => v.HasValue ? TimeOnly.FromTimeSpan(v.Value) : null);
            e.Property(x => x.PeakEndTime).HasConversion(
                v => v.HasValue ? v.Value.ToTimeSpan() : (TimeSpan?)null,
                v => v.HasValue ? TimeOnly.FromTimeSpan(v.Value) : null);
            e.HasOne(x => x.Facility)
                .WithMany(f => f.Pricings)
                .HasForeignKey(x => x.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // FacilityMaintenance
        builder.Entity<FacilityMaintenance>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.HasOne(x => x.Facility)
                .WithMany(f => f.MaintenanceWindows)
                .HasForeignKey(x => x.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // FacilityBlockout
        builder.Entity<FacilityBlockout>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Reason).HasMaxLength(500).IsRequired();
            e.HasOne(x => x.Facility)
                .WithMany(f => f.Blockouts)
                .HasForeignKey(x => x.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });
    }
}
