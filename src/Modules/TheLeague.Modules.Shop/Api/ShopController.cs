using MediatR;
using Microsoft.AspNetCore.Mvc;
using TheLeague.Modules.Shop.Application.Commands;
using TheLeague.Modules.Shop.Application.Queries;
using TheLeague.Modules.Shop.Domain;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Infrastructure.Authorization;

namespace TheLeague.Modules.Shop.Api;

[ApiController]
[Route("api/v1/shop")]
public class ShopController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantService _tenantService;

    public ShopController(IMediator mediator, ITenantService tenantService)
    {
        _mediator = mediator;
        _tenantService = tenantService;
    }

    // GET /api/v1/shop/products
    [HttpGet("products")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(
            new GetProductsQuery(categoryId, sortBy, sortDescending, page, pageSize), ct);
        return Ok(result);
    }

    // POST /api/v1/shop/products
    [HttpPost("products")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetProductById), new { id = result.Data!.Id }, result);
    }

    // GET /api/v1/shop/products/{id}
    [HttpGet("products/{id:guid}")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetProductById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id), ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    // PUT /api/v1/shop/products/{id}
    [HttpPut("products/{id:guid}")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var command = new UpdateProductCommand(id, request.Name, request.Description, request.Price, request.IsActive, request.CategoryId);
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return NotFound(result);

        return Ok(result);
    }

    // POST /api/v1/shop/products/{id}/variants
    [HttpPost("products/{id:guid}/variants")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> CreateVariant(Guid id, [FromBody] CreateVariantRequest request, CancellationToken ct)
    {
        var command = new CreateProductVariantCommand(id, request.Size, request.Color, request.StockQuantity, request.Sku);
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetProductById), new { id }, result);
    }

    // GET /api/v1/shop/categories
    [HttpGet("categories")]
    [RequireRole(Roles.ClubManager, Roles.Member)]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), ct);
        return Ok(result);
    }

    // POST /api/v1/shop/categories
    [HttpPost("categories")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Created($"/api/v1/shop/categories", result);
    }

    // POST /api/v1/shop/purchase
    [HttpPost("purchase")]
    [RequireRole(Roles.Member)]
    public async Task<IActionResult> Purchase([FromBody] PurchaseProductCommand command, CancellationToken ct)
    {
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Created($"/api/v1/shop/orders/{result.Data!.Id}", result);
    }

    // GET /api/v1/shop/orders
    [HttpGet("orders")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetOrdersQuery(page, pageSize), ct);
        return Ok(result);
    }

    // GET /api/v1/shop/orders/member/{memberId}
    [HttpGet("orders/member/{memberId:guid}")]
    [RequireRole(Roles.Member, Roles.ClubManager)]
    public async Task<IActionResult> GetMemberOrders(Guid memberId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetMemberOrdersQuery(memberId, page, pageSize), ct);
        return Ok(result);
    }

    // POST /api/v1/shop/orders/{id}/status
    [HttpPost("orders/{id:guid}/status")]
    [RequireRole(Roles.ClubManager)]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var command = new UpdateOrderStatusCommand(id, request.NewStatus);
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    // POST /api/v1/shop/products/{id}/restock-notify
    [HttpPost("products/{id:guid}/restock-notify")]
    [RequireRole(Roles.Member)]
    public async Task<IActionResult> RequestRestockNotification(Guid id, [FromBody] RestockNotifyRequest request, CancellationToken ct)
    {
        var command = new RequestRestockNotificationCommand(id, request.VariantId, request.MemberId);
        var result = await _mediator.Send(command, ct);
        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}

public record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    bool IsActive,
    Guid? CategoryId);

public record CreateVariantRequest(
    string? Size,
    string? Color,
    int StockQuantity,
    string? Sku);

public record UpdateOrderStatusRequest(
    OrderStatus NewStatus);

public record RestockNotifyRequest(
    Guid VariantId,
    Guid MemberId);
