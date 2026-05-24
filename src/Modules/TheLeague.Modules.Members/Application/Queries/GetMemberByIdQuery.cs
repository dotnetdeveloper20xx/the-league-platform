using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Members.Application.Dtos;
using TheLeague.Modules.Members.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Members.Application.Queries;

public record GetMemberByIdQuery(Guid MemberId) : IRequest<Result<MemberDto>>;

public class GetMemberByIdQueryHandler : IRequestHandler<GetMemberByIdQuery, Result<MemberDto>>
{
    private readonly MembersDbContext _db;

    public GetMemberByIdQueryHandler(MembersDbContext db)
    {
        _db = db;
    }

    public async Task<Result<MemberDto>> Handle(GetMemberByIdQuery request, CancellationToken cancellationToken)
    {
        var member = await _db.Members
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == request.MemberId, cancellationToken);

        if (member is null)
            return Result.Failure<MemberDto>("Member not found.");

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
