using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Memberships.Application.Dtos;
using TheLeague.Modules.Memberships.Domain;
using TheLeague.Modules.Memberships.Infrastructure.Persistence;
using TheLeague.Modules.Memberships.Infrastructure.Services;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Memberships.Application.Commands;

public record EnrollMemberCommand(
    Guid ClubId,
    Guid MemberId,
    Guid MembershipTypeId,
    bool AutoRenew,
    int? MemberAge,
    Guid? DiscountId
) : IRequest<Result<MembershipDto>>;

public class EnrollMemberCommandValidator : AbstractValidator<EnrollMemberCommand>
{
    public EnrollMemberCommandValidator()
    {
        RuleFor(x => x.ClubId).NotEmpty();
        RuleFor(x => x.MemberId).NotEmpty();
        RuleFor(x => x.MembershipTypeId).NotEmpty();
    }
}

public class EnrollMemberCommandHandler : IRequestHandler<EnrollMemberCommand, Result<MembershipDto>>
{
    private readonly MembershipsDbContext _db;
    private readonly IIntegrationEventBus _eventBus;

    public EnrollMemberCommandHandler(MembershipsDbContext db, IIntegrationEventBus eventBus)
    {
        _db = db;
        _eventBus = eventBus;
    }

    public async Task<Result<MembershipDto>> Handle(EnrollMemberCommand request, CancellationToken cancellationToken)
    {
        var membershipType = await _db.MembershipTypes
            .FirstOrDefaultAsync(x => x.Id == request.MembershipTypeId, cancellationToken);

        if (membershipType is null)
            return Result.Failure<MembershipDto>("Membership type not found.");

        if (!membershipType.IsActive)
            return Result.Failure<MembershipDto>("Membership type is not active.");

        // Validate age limits
        if (request.MemberAge.HasValue)
        {
            if (membershipType.MinAge.HasValue && request.MemberAge.Value < membershipType.MinAge.Value)
                return Result.Failure<MembershipDto>($"Member's age ({request.MemberAge.Value}) is below the minimum age limit ({membershipType.MinAge.Value}).");

            if (membershipType.MaxAge.HasValue && request.MemberAge.Value > membershipType.MaxAge.Value)
                return Result.Failure<MembershipDto>($"Member's age ({request.MemberAge.Value}) exceeds the maximum age limit ({membershipType.MaxAge.Value}).");
        }

        // Check capacity
        if (membershipType.Capacity.HasValue)
        {
            var activeCount = await _db.Memberships
                .CountAsync(x => x.MembershipTypeId == request.MembershipTypeId
                    && x.Status == MembershipStatus.Active, cancellationToken);

            if (activeCount >= membershipType.Capacity.Value)
                return Result.Failure<MembershipDto>("Membership type is at full capacity. Member has been added to the waitlist.");
        }

        // Calculate pricing
        var price = membershipType.Price;
        decimal? discountApplied = null;
        DiscountType? discountType = null;

        if (request.DiscountId.HasValue)
        {
            var discount = await _db.MembershipDiscounts
                .FirstOrDefaultAsync(x => x.Id == request.DiscountId.Value, cancellationToken);

            if (discount is not null && discount.IsValid(DateTime.UtcNow))
            {
                discountApplied = discount.CalculateDiscount(price);
                discountType = discount.DiscountType;
                price -= discountApplied.Value;
                if (price < 0) price = 0;
                discount.IncrementUses();
            }
        }

        // Calculate dates
        var startDate = DateTime.UtcNow;
        var endDate = BillingCycleCalculator.CalculateEndDate(startDate, membershipType.BillingCycle);

        var membership = Membership.Create(
            request.ClubId,
            request.MemberId,
            request.MembershipTypeId,
            startDate,
            endDate,
            request.AutoRenew,
            price,
            discountApplied,
            discountType);

        _db.Memberships.Add(membership);
        await _db.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(
            new MembershipEnrolledEvent(membership.Id, request.MemberId, request.ClubId, request.MembershipTypeId),
            cancellationToken);

        var dto = new MembershipDto(
            membership.Id, membership.ClubId, membership.MemberId, membership.MembershipTypeId,
            membership.StartDate, membership.EndDate, membership.Status, membership.AutoRenew,
            membership.PricePaid, membership.DiscountApplied, membership.DiscountType,
            membership.CreatedAt, membership.UpdatedAt);

        return Result.Success(dto);
    }
}
