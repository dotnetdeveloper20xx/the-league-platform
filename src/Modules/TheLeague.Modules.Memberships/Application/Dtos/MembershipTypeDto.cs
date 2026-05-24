using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Memberships.Application.Dtos;

public record MembershipTypeDto(
    Guid Id,
    Guid ClubId,
    string Name,
    string? Description,
    decimal Price,
    BillingCycle BillingCycle,
    int? MinAge,
    int? MaxAge,
    int? Capacity,
    decimal? JoiningFee,
    bool IsActive,
    bool AllowAutoRenewal,
    decimal? FreezeFee,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record MembershipDto(
    Guid Id,
    Guid ClubId,
    Guid MemberId,
    Guid MembershipTypeId,
    DateTime StartDate,
    DateTime EndDate,
    MembershipStatus Status,
    bool AutoRenew,
    decimal PricePaid,
    decimal? DiscountApplied,
    DiscountType? DiscountType,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record MembershipWaitlistDto(
    Guid Id,
    Guid MembershipTypeId,
    Guid MemberId,
    int Position,
    DateTime RequestedAt,
    DateTime? NotifiedAt,
    string Status
);

public record MembershipDiscountDto(
    Guid Id,
    Guid ClubId,
    Guid? MembershipTypeId,
    DiscountType DiscountType,
    bool IsPercentage,
    decimal Value,
    DateTime ValidFrom,
    DateTime ValidTo,
    string? PromoCode,
    int? MaxUses,
    int CurrentUses
);
