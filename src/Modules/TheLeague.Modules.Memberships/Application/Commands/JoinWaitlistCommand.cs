using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Memberships.Application.Dtos;
using TheLeague.Modules.Memberships.Domain;
using TheLeague.Modules.Memberships.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Memberships.Application.Commands;

public record JoinWaitlistCommand(
    Guid ClubId,
    Guid MembershipTypeId,
    Guid MemberId
) : IRequest<Result<MembershipWaitlistDto>>;

public class JoinWaitlistCommandValidator : AbstractValidator<JoinWaitlistCommand>
{
    public JoinWaitlistCommandValidator()
    {
        RuleFor(x => x.ClubId).NotEmpty();
        RuleFor(x => x.MembershipTypeId).NotEmpty();
        RuleFor(x => x.MemberId).NotEmpty();
    }
}

public class JoinWaitlistCommandHandler : IRequestHandler<JoinWaitlistCommand, Result<MembershipWaitlistDto>>
{
    private readonly MembershipsDbContext _db;

    public JoinWaitlistCommandHandler(MembershipsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<MembershipWaitlistDto>> Handle(JoinWaitlistCommand request, CancellationToken cancellationToken)
    {
        // Check if member is already on the waitlist
        var existing = await _db.MembershipWaitlists
            .AnyAsync(x => x.MembershipTypeId == request.MembershipTypeId
                && x.MemberId == request.MemberId
                && x.Status == WaitlistStatus.Waiting, cancellationToken);

        if (existing)
            return Result.Failure<MembershipWaitlistDto>("Member is already on the waitlist for this membership type.");

        // Get next position
        var maxPosition = await _db.MembershipWaitlists
            .Where(x => x.MembershipTypeId == request.MembershipTypeId)
            .MaxAsync(x => (int?)x.Position, cancellationToken) ?? 0;

        var waitlistEntry = MembershipWaitlist.Create(
            request.ClubId,
            request.MembershipTypeId,
            request.MemberId,
            maxPosition + 1);

        _db.MembershipWaitlists.Add(waitlistEntry);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new MembershipWaitlistDto(
            waitlistEntry.Id, waitlistEntry.MembershipTypeId, waitlistEntry.MemberId,
            waitlistEntry.Position, waitlistEntry.RequestedAt, waitlistEntry.NotifiedAt,
            waitlistEntry.Status);

        return Result.Success(dto);
    }
}
