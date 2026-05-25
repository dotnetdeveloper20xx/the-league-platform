using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Facilities.Domain;
using TheLeague.Modules.Facilities.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Facilities.Application.Commands;

public record CreateBlockoutCommand(
    Guid FacilityId,
    string Reason,
    DateTime StartDate,
    DateTime EndDate
) : IRequest<Result<Guid>>;

public class CreateBlockoutCommandHandler : IRequestHandler<CreateBlockoutCommand, Result<Guid>>
{
    private readonly FacilitiesDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateBlockoutCommandHandler(FacilitiesDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<Guid>> Handle(CreateBlockoutCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId;
        if (clubId == null)
            return Result.Failure<Guid>("Tenant context is required.");

        var facility = await _db.Facilities.FirstOrDefaultAsync(f => f.Id == request.FacilityId, cancellationToken);
        if (facility == null)
            return Result.Failure<Guid>("Facility not found.");

        if (request.EndDate <= request.StartDate)
            return Result.Failure<Guid>("End date must be after start date.");

        var blockout = FacilityBlockout.Create(
            clubId.Value, request.FacilityId, request.Reason,
            request.StartDate, request.EndDate);

        _db.FacilityBlockouts.Add(blockout);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(blockout.Id);
    }
}
