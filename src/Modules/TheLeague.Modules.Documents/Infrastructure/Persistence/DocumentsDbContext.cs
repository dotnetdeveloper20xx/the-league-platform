using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Documents.Domain;
using TheLeague.Shared.Contracts.Services;

namespace TheLeague.Modules.Documents.Infrastructure.Persistence;

public class DocumentsDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public DbSet<Document> Documents => Set<Document>();

    public DocumentsDbContext(DbContextOptions<DocumentsDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("documents");

        var tenantId = _tenantService.CurrentTenantId ?? Guid.Empty;

        builder.Entity<Document>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FileName).HasMaxLength(500).IsRequired();
            e.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
            e.Property(x => x.FileSize).IsRequired();
            e.Property(x => x.DocumentType).HasMaxLength(50).IsRequired();
            e.Property(x => x.BlobKey).HasMaxLength(500).IsRequired();
            e.Property(x => x.ThumbnailBlobKey).HasMaxLength(500);
            e.Property(x => x.UploadedByUserId).IsRequired();
            e.Property(x => x.UploadedAt).IsRequired();

            // Unique index on BlobKey
            e.HasIndex(x => x.BlobKey).IsUnique();

            // Composite indexes for common queries
            e.HasIndex(x => new { x.ClubId, x.MemberId });
            e.HasIndex(x => new { x.ClubId, x.DocumentType });

            // Tenant query filter
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });
    }
}
