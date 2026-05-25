using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Shop.Domain;
using TheLeague.Modules.Shop.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Shop.Application.Commands;

public record RequestRestockNotificationCommand(
    Guid ProductId,
    Guid VariantId,
    Guid MemberId
) : IRequest<Result<string>>;

public class RequestRestockNotificationCommandHandler : IRequestHandler<RequestRestockNotificationCommand, Result<string>>
{
    private readonly ShopDbContext _db;
    private readonly ITenantService _tenantService;

    public RequestRestockNotificationCommandHandler(ShopDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<string>> Handle(RequestRestockNotificationCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        var variantExists = await _db.ProductVariants
            .AnyAsync(v => v.Id == request.VariantId && v.ProductId == request.ProductId, cancellationToken);

        if (!variantExists)
            return Result.Failure<string>("Product variant not found");

        var existingNotification = await _db.RestockNotifications
            .AnyAsync(n => n.ProductVariantId == request.VariantId && n.MemberId == request.MemberId && n.NotifiedAt == null, cancellationToken);

        if (existingNotification)
            return Result.Success<string>("Already registered for restock notification");

        var notification = RestockNotification.Create(clubId, request.VariantId, request.MemberId);
        _db.RestockNotifications.Add(notification);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success<string>("Registered for restock notification");
    }
}
