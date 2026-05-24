using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Memberships.Application.Dtos;
using TheLeague.Modules.Memberships.Domain;
using TheLeague.Modules.Memberships.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Memberships.Application.Commands;

public record FreezeMembershipCommand(
    Guid MembershipId,
    int DurationDays,
    string? Reason
) : IRequest<Result<MembershipDto>>;

public class FreezeMembershipCommandValidator : AbstractValidator<FreezeMembershipCommand>
{
    public FreezeMembershipCommandValidator()
    {
        RuleFor(x => x.MembershipId).NotEmpty();
        RuleFor(x => x.DurationDays).InclusiveBetween(1, 365);
    }
}

public class FreezeMembershipCommandHandler : IRequestHandler<FreezeMembershipCommand, Result<MembershipDto>>
{
    private readonly MembershipsDbContext _db;

    public FreezeMembershipCommandHandler(MembershipsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<MembershipDto>> Handle(FreezeMembershipCommand request, CancellationToken cancellationToken)
    {
        var membership = await _db.Memberships
            .FirstOrDefaultAsync(x => x.Id == request.MembershipId, cancellationToken);

        if (membership is null)
            return Result.Failure<MembershipDto>("Membership not found.");

        var membershipType = await _db.MembershipTypes
            .FirstOrDefaultAsync(x => x.Id == membership.MembershipTypeId, cancellationToken);

        var freezeFee = membershipType?.FreezeFee ?? 0m;

        membership.Freeze();

        var freeze = MembershipFreeze.Create(
            membership.ClubId,
            membership.Id,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(request.DurationDays),
            freezeFee,
            request.Reason);

        _db.MembershipFreezes.Add(freeze);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new MembershipDto(
            membership.Id, membership.ClubId, membership.MemberId, membership.MembershipTypeId,
            membership.StartDate, membership.EndDate, membership.Status, membership.AutoRenew,
            membership.PricePaid, membership.DiscountApplied, membership.DiscountType,
            membership.CreatedAt, membership.UpdatedAt);

        return Result.Success(dto);
    }
}
