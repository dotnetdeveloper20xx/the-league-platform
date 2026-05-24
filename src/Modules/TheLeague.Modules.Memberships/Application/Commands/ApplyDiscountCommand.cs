using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Memberships.Application.Dtos;
using TheLeague.Modules.Memberships.Domain;
using TheLeague.Modules.Memberships.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Memberships.Application.Commands;

public record ApplyDiscountCommand(
    Guid ClubId,
    Guid? MembershipTypeId,
    DiscountType DiscountType,
    bool IsPercentage,
    decimal Value,
    DateTime ValidFrom,
    DateTime ValidTo,
    string? PromoCode,
    int? MaxUses
) : IRequest<Result<MembershipDiscountDto>>;

public class ApplyDiscountCommandValidator : AbstractValidator<ApplyDiscountCommand>
{
    public ApplyDiscountCommandValidator()
    {
        RuleFor(x => x.ClubId).NotEmpty();
        RuleFor(x => x.Value).GreaterThan(0);
        RuleFor(x => x.Value)
            .InclusiveBetween(0.01m, 100m)
            .When(x => x.IsPercentage)
            .WithMessage("Percentage discount must be between 0.01 and 100.");
        RuleFor(x => x.Value)
            .InclusiveBetween(0.01m, 999999.99m)
            .When(x => !x.IsPercentage)
            .WithMessage("Fixed discount must be between 0.01 and 999,999.99.");
        RuleFor(x => x.ValidTo).GreaterThan(x => x.ValidFrom);
        RuleFor(x => x.MaxUses).GreaterThan(0).When(x => x.MaxUses.HasValue);
    }
}

public class ApplyDiscountCommandHandler : IRequestHandler<ApplyDiscountCommand, Result<MembershipDiscountDto>>
{
    private readonly MembershipsDbContext _db;

    public ApplyDiscountCommandHandler(MembershipsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<MembershipDiscountDto>> Handle(ApplyDiscountCommand request, CancellationToken cancellationToken)
    {
        var discount = MembershipDiscount.Create(
            request.ClubId,
            request.MembershipTypeId,
            request.DiscountType,
            request.IsPercentage,
            request.Value,
            request.ValidFrom,
            request.ValidTo,
            request.PromoCode,
            request.MaxUses);

        _db.MembershipDiscounts.Add(discount);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new MembershipDiscountDto(
            discount.Id, discount.ClubId, discount.MembershipTypeId,
            discount.DiscountType, discount.IsPercentage, discount.Value,
            discount.ValidFrom, discount.ValidTo, discount.PromoCode,
            discount.MaxUses, discount.CurrentUses);

        return Result.Success(dto);
    }
}
