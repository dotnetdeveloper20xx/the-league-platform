using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Members.Application.Dtos;
using TheLeague.Modules.Members.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;
using TheLeague.Shared.Domain.ValueObjects;

namespace TheLeague.Modules.Members.Application.Commands;

public record UpdateMemberCommand(
    Guid MemberId,
    string FirstName,
    string LastName,
    string Email,
    string? Phone,
    DateTime? DateOfBirth,
    Gender? Gender,
    Address? Address,
    EmergencyContact? PrimaryEmergencyContact,
    EmergencyContact? SecondaryEmergencyContact,
    MedicalInfo? MedicalInfo,
    string? ProfilePhotoUrl,
    string? FacebookUrl,
    string? TwitterHandle,
    string? InstagramHandle,
    string? LinkedInUrl,
    string? CustomFieldValues,
    bool MarketingOptIn,
    bool SmsOptIn,
    bool EmailOptIn
) : IRequest<Result<MemberDto>>;

public class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand, Result<MemberDto>>
{
    private readonly MembersDbContext _db;

    public UpdateMemberCommandHandler(MembersDbContext db)
    {
        _db = db;
    }

    public async Task<Result<MemberDto>> Handle(UpdateMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken);

        if (member is null)
            return Result.Failure<MemberDto>("Member not found.");

        member.Update(
            request.FirstName, request.LastName, request.Email,
            request.Phone, request.DateOfBirth, request.Gender,
            request.Address, request.PrimaryEmergencyContact,
            request.SecondaryEmergencyContact, request.MedicalInfo,
            request.ProfilePhotoUrl, request.FacebookUrl,
            request.TwitterHandle, request.InstagramHandle, request.LinkedInUrl,
            request.CustomFieldValues, request.MarketingOptIn,
            request.SmsOptIn, request.EmailOptIn);

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new MemberDto(
            member.Id, member.ClubId, member.UserId, member.MemberNumber,
            member.FirstName, member.LastName, member.Email, member.Phone,
            member.DateOfBirth, member.Gender, member.Address,
            member.PrimaryEmergencyContact, member.SecondaryEmergencyContact,
            member.MedicalInfo, member.ProfilePhotoUrl,
            member.FacebookUrl, member.TwitterHandle, member.InstagramHandle, member.LinkedInUrl,
            member.CustomFieldValues, member.MarketingOptIn, member.SmsOptIn, member.EmailOptIn,
            member.IsFamilyAccount, member.PrimaryMemberId, member.Status, member.JoinedDate,
            member.LastLoginDate, member.IsActive, member.QRCodeData,
            member.ReferredByMemberId, member.ReferralSource,
            member.CreatedAt, member.UpdatedAt
        ));
    }
}
