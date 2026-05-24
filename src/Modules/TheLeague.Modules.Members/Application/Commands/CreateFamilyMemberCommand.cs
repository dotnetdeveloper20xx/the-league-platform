using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Members.Application.Dtos;
using TheLeague.Modules.Members.Domain;
using TheLeague.Modules.Members.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Members.Application.Commands;

public record CreateFamilyMemberCommand(
    Guid PrimaryMemberId,
    Guid DependentMemberId,
    FamilyMemberRelation Relationship
) : IRequest<Result<FamilyMemberDto>>;

public class CreateFamilyMemberCommandHandler : IRequestHandler<CreateFamilyMemberCommand, Result<FamilyMemberDto>>
{
    private readonly MembersDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateFamilyMemberCommandHandler(MembersDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<FamilyMemberDto>> Handle(CreateFamilyMemberCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        // Validate max 10 dependents
        var existingCount = await _db.FamilyMembers
            .CountAsync(f => f.PrimaryMemberId == request.PrimaryMemberId, cancellationToken);

        if (existingCount >= 10)
            return Result.Failure<FamilyMemberDto>("A primary member can have a maximum of 10 family members.");

        // Validate both members exist
        var primaryMember = await _db.Members
            .FirstOrDefaultAsync(m => m.Id == request.PrimaryMemberId, cancellationToken);
        if (primaryMember is null)
            return Result.Failure<FamilyMemberDto>("Primary member not found.");

        var dependentMember = await _db.Members
            .FirstOrDefaultAsync(m => m.Id == request.DependentMemberId, cancellationToken);
        if (dependentMember is null)
            return Result.Failure<FamilyMemberDto>("Dependent member not found.");

        // Check for duplicate link
        var exists = await _db.FamilyMembers
            .AnyAsync(f => f.PrimaryMemberId == request.PrimaryMemberId
                        && f.DependentMemberId == request.DependentMemberId, cancellationToken);
        if (exists)
            return Result.Failure<FamilyMemberDto>("This family member link already exists.");

        var familyMember = FamilyMember.Create(
            clubId, request.PrimaryMemberId, request.DependentMemberId, request.Relationship);

        _db.FamilyMembers.Add(familyMember);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new FamilyMemberDto(
            familyMember.Id,
            familyMember.PrimaryMemberId,
            familyMember.DependentMemberId,
            dependentMember.FirstName,
            dependentMember.LastName,
            dependentMember.Email,
            familyMember.Relationship
        ));
    }
}
