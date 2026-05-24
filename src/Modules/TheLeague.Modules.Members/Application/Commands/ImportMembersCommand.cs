using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Members.Application.Dtos;
using TheLeague.Modules.Members.Domain;
using TheLeague.Modules.Members.Infrastructure.Persistence;
using TheLeague.Modules.Members.Infrastructure.Services;
using TheLeague.Shared.Contracts.Events;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Members.Application.Commands;

public record ImportMemberRow(
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    DateTime? DateOfBirth
);

public record ImportMembersCommand(List<ImportMemberRow> Rows) : IRequest<Result<ImportResultDto>>;

public class ImportMembersCommandHandler : IRequestHandler<ImportMembersCommand, Result<ImportResultDto>>
{
    private readonly MembersDbContext _db;
    private readonly MemberNumberGenerator _numberGenerator;
    private readonly IIntegrationEventBus _eventBus;
    private readonly ITenantService _tenantService;

    public ImportMembersCommandHandler(
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

    public async Task<Result<ImportResultDto>> Handle(ImportMembersCommand request, CancellationToken cancellationToken)
    {
        var clubId = _tenantService.CurrentTenantId!.Value;

        if (request.Rows.Count > 2000)
            return Result.Failure<ImportResultDto>("Import file exceeds maximum of 2000 rows.");

        var errors = new List<ImportErrorDto>();
        var validMembers = new List<Member>();
        var emailsInBatch = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Get existing emails for this club
        var existingEmails = await _db.Members
            .Select(m => m.Email.ToLower())
            .ToListAsync(cancellationToken);
        var existingEmailSet = new HashSet<string>(existingEmails, StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < request.Rows.Count; i++)
        {
            var row = request.Rows[i];
            var rowNumber = i + 1;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(row.FirstName))
            {
                errors.Add(new ImportErrorDto(rowNumber, "FirstName", "First name is required."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(row.LastName))
            {
                errors.Add(new ImportErrorDto(rowNumber, "LastName", "Last name is required."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(row.Email))
            {
                errors.Add(new ImportErrorDto(rowNumber, "Email", "Email is required."));
                continue;
            }

            if (row.FirstName.Length > 100)
            {
                errors.Add(new ImportErrorDto(rowNumber, "FirstName", "First name exceeds maximum length of 100 characters."));
                continue;
            }

            if (row.LastName.Length > 100)
            {
                errors.Add(new ImportErrorDto(rowNumber, "LastName", "Last name exceeds maximum length of 100 characters."));
                continue;
            }

            if (row.Email.Length > 256)
            {
                errors.Add(new ImportErrorDto(rowNumber, "Email", "Email exceeds maximum length of 256 characters."));
                continue;
            }

            // Check duplicate in batch
            if (!emailsInBatch.Add(row.Email))
            {
                errors.Add(new ImportErrorDto(rowNumber, "Email", "Duplicate email address within import file."));
                continue;
            }

            // Check existing in database
            if (existingEmailSet.Contains(row.Email))
            {
                errors.Add(new ImportErrorDto(rowNumber, "Email", "Email already exists in the club."));
                continue;
            }

            var member = Member.Create(clubId, row.FirstName, row.LastName, row.Email);
            validMembers.Add(member);
        }

        // Generate member numbers and save
        foreach (var member in validMembers)
        {
            var memberNumber = await _numberGenerator.GenerateNextAsync(clubId, cancellationToken);
            member.SetMemberNumber(memberNumber);
            _db.Members.Add(member);
            await _db.SaveChangesAsync(cancellationToken);

            await _eventBus.PublishAsync(
                new MemberCreatedEvent(member.Id, clubId, member.Email),
                cancellationToken);
        }

        var result = new ImportResultDto(
            request.Rows.Count,
            validMembers.Count,
            errors.Count,
            errors
        );

        return Result.Success(result);
    }
}
