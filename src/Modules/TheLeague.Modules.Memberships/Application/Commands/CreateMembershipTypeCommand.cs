using FluentValidation;
using MediatR;
using TheLeague.Modules.Memberships.Application.Dtos;
using TheLeague.Modules.Memberships.Domain;
using TheLeague.Modules.Memberships.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Memberships.Application.Commands;

public record CreateMembershipTypeCommand(
    Guid ClubId,
    string Name,
    string? Description,
    decimal Price,
    BillingCycle BillingCycle,
    int? MinAge,
    int? MaxAge,
    int? Capacity,
    decimal? JoiningFee,
    bool AllowAutoRenewal,
    decimal? FreezeFee
) : IRequest<Result<MembershipTypeDto>>;

public class CreateMembershipTypeCommandValidator : AbstractValidator<CreateMembershipTypeCommand>
{
    public CreateMembershipTypeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.Price).InclusiveBetween(0m, 999999.99m);
        RuleFor(x => x.MinAge).InclusiveBetween(0, 120).When(x => x.MinAge.HasValue);
        RuleFor(x => x.MaxAge).InclusiveBetween(0, 120).When(x => x.MaxAge.HasValue);
        RuleFor(x => x.MaxAge)
            .GreaterThanOrEqualTo(x => x.MinAge)
            .When(x => x.MinAge.HasValue && x.MaxAge.HasValue)
            .WithMessage("Maximum age must be greater than or equal to minimum age.");
        RuleFor(x => x.Capacity).InclusiveBetween(1, 10000).When(x => x.Capacity.HasValue);
        RuleFor(x => x.JoiningFee).GreaterThanOrEqualTo(0).When(x => x.JoiningFee.HasValue);
        RuleFor(x => x.FreezeFee).GreaterThanOrEqualTo(0).When(x => x.FreezeFee.HasValue);
    }
}

public class CreateMembershipTypeCommandHandler : IRequestHandler<CreateMembershipTypeCommand, Result<MembershipTypeDto>>
{
    private readonly MembershipsDbContext _db;

    public CreateMembershipTypeCommandHandler(MembershipsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<MembershipTypeDto>> Handle(CreateMembershipTypeCommand request, CancellationToken cancellationToken)
    {
        var membershipType = MembershipType.Create(request.ClubId, request.Name, request.Price, request.BillingCycle);

        membershipType.Update(
            request.Name,
            request.Description,
            request.Price,
            request.BillingCycle,
            request.MinAge,
            request.MaxAge,
            request.Capacity,
            request.JoiningFee,
            true,
            request.AllowAutoRenewal,
            request.FreezeFee);

        _db.MembershipTypes.Add(membershipType);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new MembershipTypeDto(
            membershipType.Id, membershipType.ClubId, membershipType.Name, membershipType.Description,
            membershipType.Price, membershipType.BillingCycle, membershipType.MinAge, membershipType.MaxAge,
            membershipType.Capacity, membershipType.JoiningFee, membershipType.IsActive,
            membershipType.AllowAutoRenewal, membershipType.FreezeFee,
            membershipType.CreatedAt, membershipType.UpdatedAt);

        return Result.Success(dto);
    }
}
