using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Programs.Domain;
using TheLeague.Shared.Contracts.Services;

namespace TheLeague.Modules.Programs.Infrastructure.Persistence;

public class ProgramsDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public DbSet<Program> Programs => Set<Program>();
    public DbSet<ProgramSession> ProgramSessions => Set<ProgramSession>();
    public DbSet<ProgramEnrollment> ProgramEnrollments => Set<ProgramEnrollment>();
    public DbSet<ProgramAttendance> ProgramAttendances => Set<ProgramAttendance>();
    public DbSet<MemberCertificate> MemberCertificates => Set<MemberCertificate>();

    public ProgramsDbContext(DbContextOptions<ProgramsDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("programs");

        var tenantId = _tenantService.CurrentTenantId ?? Guid.Empty;

        // Program
        builder.Entity<Program>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.HasIndex(x => new { x.ClubId, x.IsActive });
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // ProgramSession
        builder.Entity<ProgramSession>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.InstructorName).HasMaxLength(200);
            e.Property(x => x.VenueName).HasMaxLength(200);
            e.HasOne(x => x.Program)
                .WithMany(p => p.Sessions)
                .HasForeignKey(x => x.ProgramId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.ProgramId, x.SessionOrder });
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // ProgramEnrollment
        builder.Entity<ProgramEnrollment>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Program)
                .WithMany(p => p.Enrollments)
                .HasForeignKey(x => x.ProgramId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.ProgramId, x.MemberId });
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // ProgramAttendance
        builder.Entity<ProgramAttendance>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Session)
                .WithMany(s => s.AttendanceRecords)
                .HasForeignKey(x => x.ProgramSessionId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.ProgramSessionId, x.MemberId }).IsUnique();
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // MemberCertificate
        builder.Entity<MemberCertificate>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ProgramName).HasMaxLength(200).IsRequired();
            e.Property(x => x.CertificateNumber).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.CertificateNumber).IsUnique();
            e.HasIndex(x => new { x.ClubId, x.MemberId });
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });
    }
}
