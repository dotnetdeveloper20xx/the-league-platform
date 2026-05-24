using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Memberships.Domain;
using TheLeague.Shared.Contracts.Services;

namespace TheLeague.Modules.Memberships.Infrastructure.Persistence;

public class MembershipsDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public DbSet<MembershipType> MembershipTypes => Set<MembershipType>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<MembershipDiscount> MembershipDiscounts => Set<MembershipDiscount>();
    public DbSet<MembershipFreeze> MembershipFreezes => Set<MembershipFreeze>();
    public DbSet<MembershipWaitlist> MembershipWaitlists => Set<MembershipWaitlist>();
    public DbSet<GuestPass> GuestPasses => Set<GuestPass>();

    public MembershipsDbContext(DbContextOptions<MembershipsDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("memberships");

        // Tenant query filters
        builder.Entity<MembershipType>().HasQueryFilter(x => x.ClubId == _tenantService.CurrentTenantId);
        builder.Entity<Membership>().HasQueryFilter(x => x.ClubId == _tenantService.CurrentTenantId);
        builder.Entity<MembershipDiscount>().HasQueryFilter(x => x.ClubId == _tenantService.CurrentTenantId);
        builder.Entity<MembershipFreeze>().HasQueryFilter(x => x.ClubId == _tenantService.CurrentTenantId);
        builder.Entity<MembershipWaitlist>().HasQueryFilter(x => x.ClubId == _tenantService.CurrentTenantId);
        builder.Entity<GuestPass>().HasQueryFilter(x => x.ClubId == _tenantService.CurrentTenantId);

        // MembershipType configuration
        builder.Entity<MembershipType>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.Property(x => x.JoiningFee).HasColumnType("decimal(18,2)");
            e.Property(x => x.FreezeFee).HasColumnType("decimal(18,2)");
        });

        // Membership configuration
        builder.Entity<Membership>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PricePaid).HasColumnType("decimal(18,2)");
            e.Property(x => x.DiscountApplied).HasColumnType("decimal(18,2)");
            e.HasIndex(x => new { x.ClubId, x.MemberId, x.Status });
        });

        // MembershipDiscount configuration
        builder.Entity<MembershipDiscount>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Value).HasColumnType("decimal(18,2)");
            e.Property(x => x.PromoCode).HasMaxLength(50);
        });

        // MembershipFreeze configuration
        builder.Entity<MembershipFreeze>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FeeCharged).HasColumnType("decimal(18,2)");
            e.Property(x => x.Reason).HasMaxLength(500);
        });

        // MembershipWaitlist configuration
        builder.Entity<MembershipWaitlist>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasMaxLength(20);
            e.HasIndex(x => new { x.ClubId, x.MembershipTypeId, x.Position });
        });

        // GuestPass configuration
        builder.Entity<GuestPass>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PassCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
        });
    }
}
