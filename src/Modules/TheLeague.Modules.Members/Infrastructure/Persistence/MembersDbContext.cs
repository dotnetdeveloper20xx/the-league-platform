using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Members.Domain;
using TheLeague.Shared.Contracts.Services;

namespace TheLeague.Modules.Members.Infrastructure.Persistence;

public class MembersDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public DbSet<Member> Members => Set<Member>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<CustomFieldDefinition> CustomFieldDefinitions => Set<CustomFieldDefinition>();
    public DbSet<MemberNote> MemberNotes => Set<MemberNote>();
    public DbSet<MemberStatusTransition> MemberStatusTransitions => Set<MemberStatusTransition>();

    public MembersDbContext(DbContextOptions<MembersDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("members");

        var tenantId = _tenantService.CurrentTenantId ?? Guid.Empty;

        // Member configuration
        builder.Entity<Member>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            e.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(50);
            e.Property(x => x.MemberNumber).HasMaxLength(20).IsRequired();
            e.Property(x => x.ProfilePhotoUrl).HasMaxLength(500);
            e.Property(x => x.FacebookUrl).HasMaxLength(500);
            e.Property(x => x.TwitterHandle).HasMaxLength(100);
            e.Property(x => x.InstagramHandle).HasMaxLength(100);
            e.Property(x => x.LinkedInUrl).HasMaxLength(500);
            e.Property(x => x.QRCodeData).HasMaxLength(500);
            e.Property(x => x.ReferralSource).HasMaxLength(200);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Gender).HasConversion<string>().HasMaxLength(20);

            // Value object owned types
            e.OwnsOne(x => x.Address, a =>
            {
                a.Property(p => p.Line1).HasMaxLength(200).HasColumnName("Address_Line1");
                a.Property(p => p.Line2).HasMaxLength(200).HasColumnName("Address_Line2");
                a.Property(p => p.City).HasMaxLength(100).HasColumnName("Address_City");
                a.Property(p => p.County).HasMaxLength(100).HasColumnName("Address_County");
                a.Property(p => p.PostCode).HasMaxLength(20).HasColumnName("Address_PostCode");
                a.Property(p => p.Country).HasMaxLength(100).HasColumnName("Address_Country");
            });

            e.OwnsOne(x => x.PrimaryEmergencyContact, ec =>
            {
                ec.Property(p => p.Name).HasMaxLength(200).HasColumnName("PrimaryEmergency_Name");
                ec.Property(p => p.Phone).HasMaxLength(50).HasColumnName("PrimaryEmergency_Phone");
                ec.Property(p => p.Relation).HasMaxLength(50).HasColumnName("PrimaryEmergency_Relation");
            });

            e.OwnsOne(x => x.SecondaryEmergencyContact, ec =>
            {
                ec.Property(p => p.Name).HasMaxLength(200).HasColumnName("SecondaryEmergency_Name");
                ec.Property(p => p.Phone).HasMaxLength(50).HasColumnName("SecondaryEmergency_Phone");
                ec.Property(p => p.Relation).HasMaxLength(50).HasColumnName("SecondaryEmergency_Relation");
            });

            e.OwnsOne(x => x.MedicalInfo, mi =>
            {
                mi.Property(p => p.Conditions).HasMaxLength(1000).HasColumnName("Medical_Conditions");
                mi.Property(p => p.Allergies).HasMaxLength(1000).HasColumnName("Medical_Allergies");
                mi.Property(p => p.DoctorName).HasMaxLength(200).HasColumnName("Medical_DoctorName");
                mi.Property(p => p.DoctorPhone).HasMaxLength(50).HasColumnName("Medical_DoctorPhone");
                mi.Property(p => p.BloodType).HasMaxLength(10).HasColumnName("Medical_BloodType");
            });

            // Indexes
            e.HasIndex(x => new { x.ClubId, x.Email }).IsUnique();
            e.HasIndex(x => new { x.ClubId, x.MemberNumber }).IsUnique();
            e.HasIndex(x => new { x.ClubId, x.Status });

            // Tenant query filter
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // FamilyMember configuration
        builder.Entity<FamilyMember>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Relationship).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(x => new { x.PrimaryMemberId, x.DependentMemberId }).IsUnique();
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // CustomFieldDefinition configuration
        builder.Entity<CustomFieldDefinition>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.FieldType).HasMaxLength(50).IsRequired();
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // MemberNote configuration
        builder.Entity<MemberNote>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.NoteType).HasMaxLength(50).IsRequired();
            e.Property(x => x.Content).IsRequired();
            e.Property(x => x.CreatedByUserId).HasMaxLength(100);
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // MemberStatusTransition configuration
        builder.Entity<MemberStatusTransition>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PreviousStatus).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.NewStatus).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.ChangedByUserId).HasMaxLength(100);
            e.HasIndex(x => new { x.MemberId, x.ChangedAt });
        });
    }
}
