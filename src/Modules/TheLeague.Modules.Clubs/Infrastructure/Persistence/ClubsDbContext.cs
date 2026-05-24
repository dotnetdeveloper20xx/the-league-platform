using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Clubs.Domain;

namespace TheLeague.Modules.Clubs.Infrastructure.Persistence;

public class ClubsDbContext : DbContext
{
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<ClubSettings> ClubSettings => Set<ClubSettings>();
    public DbSet<SportConfiguration> SportConfigurations => Set<SportConfiguration>();

    public ClubsDbContext(DbContextOptions<ClubsDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("clubs");

        builder.Entity<Club>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(100).IsRequired();
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.PrimaryColor).HasMaxLength(20);
            e.Property(x => x.SecondaryColor).HasMaxLength(20);
        });

        builder.Entity<ClubSettings>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ClubId).IsUnique();
        });

        builder.Entity<SportConfiguration>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ClubId).IsUnique();
        });
    }
}
