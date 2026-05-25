using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Programs.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Programs.Application.Commands;

public record WithdrawEnrollmentCommand(
    Guid ProgramId,
    Guid MemberId
) : IRequest<Result>;

public class WithdrawEnrollmentCommandHandler : IRequestHandler<WithdrawEnrollmentCommand, Result>
{
    private readonly ProgramsDbContext _db;

    public WithdrawEnrollmentCommandHandler(ProgramsDbContext db)
    {
        _db = db;
    }

    public async Task<Result> Handle(WithdrawEnrollmentCommand request, CancellationToken cancellationToken)
    {
        var enrollment = await _db.ProgramEnrollments
            .FirstOrDefaultAsync(e => e.ProgramId == request.ProgramId && e.MemberId == request.MemberId
                && (e.Status == EnrollmentStatus.Confirmed || e.Status == EnrollmentStatus.WaitListed),
                cancellationToken);

        if (enrollment is null)
            return Result.Failure("Active enrollment not found for this member and program.");

        enrollment.Withdraw();
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success("Enrollment withdrawn successfully.");
    }
}
