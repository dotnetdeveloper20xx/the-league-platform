using TheLeague.Modules.Shop.Domain;

namespace TheLeague.Modules.Shop.Application.Dtos;

public record ProductListDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    bool IsActive,
    Guid? CategoryId,
    string? CategoryName,
    int VariantCount,
    DateTime CreatedAt);

public record ProductDetailDto(
    Guid Id,
    string Name,
    string? Description,
    decimal Price,
    bool IsActive,
    Guid? CategoryId,
    string? CategoryName,
    List<ProductVariantDto> Variants,
    List<ProductImageDto> Images,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record ProductVariantDto(
    Guid Id,
    string? Size,
    string? Color,
    int StockQuantity,
    string? Sku,
    bool IsActive);

public record ProductImageDto(
    Guid Id,
    string ImageUrl,
    int DisplayOrder,
    bool IsMain);

public record ProductCategoryDto(
    Guid Id,
    string Name,
    int DisplayOrder);

public record OrderDto(
    Guid Id,
    Guid MemberId,
    string OrderReference,
    OrderStatus Status,
    decimal TotalAmount,
    DateTime OrderedAt,
    DateTime? ConfirmedAt,
    DateTime? DispatchedAt,
    DateTime? DeliveredAt,
    DateTime? RefundedAt,
    List<OrderItemDto> Items);

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    Guid? VariantId,
    string ProductName,
    string? VariantDescription,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);
