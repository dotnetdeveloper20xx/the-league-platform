using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Equipment.Domain;
using TheLeague.Shared.Contracts.Services;

namespace TheLeague.Modules.Equipment.Infrastructure.Persistence;

public class EquipmentDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public DbSet<EquipmentItem> Equipment => Set<EquipmentItem>();
    public DbSet<EquipmentLoan> EquipmentLoans => Set<EquipmentLoan>();
    public DbSet<EquipmentReservation> EquipmentReservations => Set<EquipmentReservation>();
    public DbSet<EquipmentMaintenance> EquipmentMaintenanceRecords => Set<EquipmentMaintenance>();

    public EquipmentDbContext(DbContextOptions<EquipmentDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("equipment");

        var tenantId = _tenantService.CurrentTenantId ?? Guid.Empty;

        // Equipment configuration
        builder.Entity<EquipmentItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.ToTable("Equipment");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Category).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.Condition).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.Location).HasMaxLength(200);
            e.Property(x => x.Value).HasColumnType("decimal(18,2)");
            e.Property(x => x.AnnualDepreciationRate).HasColumnType("decimal(5,2)");
            e.Property(x => x.SerialNumber).HasMaxLength(200);

            e.HasIndex(x => new { x.ClubId, x.Category });
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // EquipmentLoan configuration
        builder.Entity<EquipmentLoan>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Fee).HasColumnType("decimal(18,2)");
            e.Property(x => x.Deposit).HasColumnType("decimal(18,2)");
            e.Property(x => x.Notes).HasMaxLength(1000);

            e.HasOne(x => x.Equipment)
                .WithMany(eq => eq.Loans)
                .HasForeignKey(x => x.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.EquipmentId, x.Status });
            e.HasIndex(x => new { x.ClubId, x.MemberId });
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // EquipmentReservation configuration
        builder.Entity<EquipmentReservation>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.Notes).HasMaxLength(1000);

            e.HasOne(x => x.Equipment)
                .WithMany(eq => eq.Reservations)
                .HasForeignKey(x => x.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.EquipmentId, x.Status });
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // EquipmentMaintenance configuration
        builder.Entity<EquipmentMaintenance>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            e.Property(x => x.ResultingCondition).HasConversion<string>().HasMaxLength(50);
            e.Property(x => x.Cost).HasColumnType("decimal(18,2)");
            e.Property(x => x.PerformedBy).HasMaxLength(200);

            e.HasOne(x => x.Equipment)
                .WithMany(eq => eq.MaintenanceRecords)
                .HasForeignKey(x => x.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.EquipmentId, x.MaintenanceDate });
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });
    }
}
