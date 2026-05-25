using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Equipment.Application.Dtos;
using TheLeague.Modules.Equipment.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Equipment.Application.Commands;

public record ReturnLoanCommand(
    Guid LoanId,
    bool IsDamaged = false
) : IRequest<Result<LoanDto>>;

public class ReturnLoanCommandHandler : IRequestHandler<ReturnLoanCommand, Result<LoanDto>>
{
    private readonly EquipmentDbContext _db;

    public ReturnLoanCommandHandler(EquipmentDbContext db)
    {
        _db = db;
    }

    public async Task<Result<LoanDto>> Handle(ReturnLoanCommand request, CancellationToken cancellationToken)
    {
        var loan = await _db.EquipmentLoans
            .FirstOrDefaultAsync(l => l.Id == request.LoanId, cancellationToken);

        if (loan is null)
            return Result.Failure<LoanDto>("Loan not found.");

        if (loan.Status == LoanStatus.Returned || loan.Status == LoanStatus.ReturnedDamaged)
            return Result.Failure<LoanDto>("Loan has already been returned.");

        var returnDate = DateTime.UtcNow;

        if (request.IsDamaged)
            loan.MarkReturnedDamaged(returnDate);
        else
            loan.MarkReturned(returnDate);

        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success(new LoanDto(
            loan.Id, loan.EquipmentId, loan.MemberId, loan.Status,
            loan.LoanDate, loan.ExpectedReturnDate, loan.ActualReturnDate,
            loan.Fee, loan.Deposit, loan.Notes, loan.CreatedAt));
    }
}
