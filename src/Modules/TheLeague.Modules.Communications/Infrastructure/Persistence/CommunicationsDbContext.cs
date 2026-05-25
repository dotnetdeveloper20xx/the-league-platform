using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Communications.Domain;
using TheLeague.Shared.Contracts.Services;

namespace TheLeague.Modules.Communications.Infrastructure.Persistence;

public class CommunicationsDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public DbSet<CommunicationTemplate> Templates => Set<CommunicationTemplate>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
    public DbSet<BulkEmailCampaign> BulkEmailCampaigns => Set<BulkEmailCampaign>();
    public DbSet<SmsLog> SmsLogs => Set<SmsLog>();

    public CommunicationsDbContext(DbContextOptions<CommunicationsDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("communications");

        // Tenant query filters
        builder.Entity<CommunicationTemplate>().HasQueryFilter(e => e.ClubId == _tenantService.CurrentTenantId);
        builder.Entity<EmailLog>().HasQueryFilter(e => e.ClubId == _tenantService.CurrentTenantId);
        builder.Entity<BulkEmailCampaign>().HasQueryFilter(e => e.ClubId == _tenantService.CurrentTenantId);
        builder.Entity<SmsLog>().HasQueryFilter(e => e.ClubId == _tenantService.CurrentTenantId);

        // CommunicationTemplate configuration
        builder.Entity<CommunicationTemplate>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.TemplateType).HasMaxLength(50).IsRequired();
            e.Property(x => x.Subject).HasMaxLength(200).IsRequired();
            e.Property(x => x.Body).HasMaxLength(10000).IsRequired();
            e.HasIndex(x => new { x.ClubId, x.TemplateType });
        });

        // EmailLog configuration
        builder.Entity<EmailLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.RecipientEmail).HasMaxLength(256).IsRequired();
            e.Property(x => x.TemplateType).HasMaxLength(50).IsRequired();
            e.Property(x => x.Subject).HasMaxLength(200).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.FailureReason).HasMaxLength(1000);
            e.HasIndex(x => new { x.ClubId, x.SentAt });
            e.HasIndex(x => new { x.ClubId, x.Status });
        });

        // BulkEmailCampaign configuration
        builder.Entity<BulkEmailCampaign>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Subject).HasMaxLength(200).IsRequired();
            e.Property(x => x.Body).HasMaxLength(10000).IsRequired();
            e.Property(x => x.TargetSegment).HasMaxLength(4000);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.HasIndex(x => new { x.ClubId, x.Status });
        });

        // SmsLog configuration
        builder.Entity<SmsLog>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.RecipientPhone).HasMaxLength(20).IsRequired();
            e.Property(x => x.Message).HasMaxLength(160).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.FailureReason).HasMaxLength(1000);
            e.HasIndex(x => new { x.ClubId, x.SentAt });
        });
    }
}
