using MediatR;
using TheLeague.Modules.Members.Application.Dtos;
using TheLeague.Modules.Members.Domain;
using TheLeague.Modules.Members.Infrastructure.Persistence;
using TheLeague.Modules.Members.Infrastructure.Services;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Members.Application.Commands;

public record CreateMemberCommand(
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateTime? DateOfBirth
) : IRequest<Result<MemberDto>>;

public class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, Result<MemberDto>>
{
    private readonly MembersDbContext _db;
    private readonly MemberNumberGenerator _numberGenerator;
    private readonly IIntegrationEventBus _eventBus;
    private readonly ITenantService _tenantService;

    public CreateMemberCommandHandler(
        MembersDbContext db,
        MemberNumberGenerator numberGenerator,
        IIntegrationEventBus eventBus,
        ITenantService tenantService)
    {
        _db = db;
        _numberGenerator = numberGenerator;
        _eventBus = eventBus;
        _tenantService = tenantService;
    }

    public async Task<Result<MemberDto>> Handle(CreateMemberCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        var member = Member.Create(clubId, request.FirstName, request.LastName, request.Email);

        if (request.Phone != null)
        {
            member.Update(
                request.FirstName, request.LastName, request.Email,
                request.Phone, request.DateOfBirth, null, null, null, null, null,
                null, null, null, null, null, null, false, false, true);
        }

        var memberNumber = await _numberGenerator.GenerateNextAsync(clubId, cancellationToken);
        member.SetMemberNumber(memberNumber);

        _db.Members.Add(member);
        await _db.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(
            new MemberCreatedEvent(member.Id, clubId, member.Email),
            cancellationToken);

        return Result.Success(MapToDto(member));
    }

    private static MemberDto MapToDto(Member m) => new(
        m.Id, m.ClubId, m.UserId, m.MemberNumber,
        m.FirstName, m.LastName, m.Email, m.Phone,
        m.DateOfBirth, m.Gender, m.Address,
        m.PrimaryEmergencyContact, m.SecondaryEmergencyContact,
        m.MedicalInfo, m.ProfilePhotoUrl,
        m.FacebookUrl, m.TwitterHandle, m.InstagramHandle, m.LinkedInUrl,
        m.CustomFieldValues, m.MarketingOptIn, m.SmsOptIn, m.EmailOptIn,
        m.IsFamilyAccount, m.PrimaryMemberId, m.Status, m.JoinedDate,
        m.LastLoginDate, m.IsActive, m.QRCodeData,
        m.ReferredByMemberId, m.ReferralSource,
        m.CreatedAt, m.UpdatedAt
    );
}
