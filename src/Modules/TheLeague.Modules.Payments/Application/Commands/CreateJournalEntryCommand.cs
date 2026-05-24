using MediatR;
using TheLeague.Modules.Payments.Domain;
using TheLeague.Modules.Payments.Infrastructure.Persistence;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Payments.Application.Commands;

public record JournalEntryLineRequest(
    Guid AccountId,
    decimal DebitAmount,
    decimal CreditAmount,
    string? Description = null);

public record CreateJournalEntryCommand(
    DateTime EntryDate,
    string Description,
    string? ReferenceNumber,
    List<JournalEntryLineRequest> Lines
) : IRequest<Result<Guid>>;

public class CreateJournalEntryCommandHandler : IRequestHandler<CreateJournalEntryCommand, Result<Guid>>
{
    private readonly PaymentsDbContext _db;
    private readonly ITenantService _tenantService;

    public CreateJournalEntryCommandHandler(PaymentsDbContext db, ITenantService tenantService)
    {
        _db = db;
        _tenantService = tenantService;
    }

    public async Task<Result<Guid>> Handle(CreateJournalEntryCommand request, CancellationToken ct)
    {
        var clubId = _tenantService.CurrentTenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        if (request.Lines == null || request.Lines.Count < 2)
            return Result.Failure<Guid>("A journal entry must have at least two lines.");

        var entry = JournalEntry.Create(clubId, request.EntryDate, request.Description, request.ReferenceNumber);

        foreach (var line in request.Lines)
        {
            entry.AddLine(line.AccountId, line.DebitAmount, line.CreditAmount, line.Description);
        }

        // Validate balanced entry
        if (!entry.IsBalanced())
            return Result.Failure<Guid>("Journal entry is not balanced. Total debits must equal total credits.");

        entry.Post();

        _db.JournalEntries.Add(entry);
        await _db.SaveChangesAsync(ct);

        return Result.Success(entry.Id);
    }
}
