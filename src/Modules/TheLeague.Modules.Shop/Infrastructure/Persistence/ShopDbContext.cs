using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Shop.Domain;
using TheLeague.Shared.Contracts.Services;

namespace TheLeague.Modules.Shop.Infrastructure.Persistence;

public class ShopDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<RestockNotification> RestockNotifications => Set<RestockNotification>();

    public ShopDbContext(DbContextOptions<ShopDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("shop");

        var tenantId = _tenantService.CurrentTenantId ?? Guid.Empty;

        // Product configuration
        builder.Entity<Product>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2000);
            e.Property(x => x.Price).HasPrecision(18, 2);
            e.HasMany(x => x.Variants).WithOne(x => x.Product).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Images).WithOne(x => x.Product).HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.SetNull);
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // ProductVariant configuration
        builder.Entity<ProductVariant>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Size).HasMaxLength(50);
            e.Property(x => x.Color).HasMaxLength(50);
            e.Property(x => x.Sku).HasMaxLength(100);
            e.HasIndex(x => x.ProductId);
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // ProductCategory configuration
        builder.Entity<ProductCategory>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // ProductImage configuration
        builder.Entity<ProductImage>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // Order configuration
        builder.Entity<Order>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.OrderReference).HasMaxLength(50).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.TotalAmount).HasPrecision(18, 2);
            e.HasIndex(x => new { x.ClubId, x.OrderReference }).IsUnique();
            e.HasMany(x => x.Items).WithOne(x => x.Order).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // OrderItem configuration
        builder.Entity<OrderItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.ProductName).HasMaxLength(150).IsRequired();
            e.Property(x => x.VariantDescription).HasMaxLength(200);
            e.Property(x => x.UnitPrice).HasPrecision(18, 2);
            e.Property(x => x.TotalPrice).HasPrecision(18, 2);
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });

        // RestockNotification configuration
        builder.Entity<RestockNotification>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.ProductVariantId, x.MemberId }).IsUnique();
            e.HasQueryFilter(x => x.ClubId == tenantId);
        });
    }
}
