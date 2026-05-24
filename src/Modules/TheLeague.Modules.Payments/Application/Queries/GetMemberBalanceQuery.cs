using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Payments.Application.Dtos;
using TheLeague.Modules.Payments.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Payments.Application.Queries;

public record GetMemberBalanceQuery(Guid MemberId) : IRequest<Result<MemberBalanceDto>>;

public class GetMemberBalanceQueryHandler : IRequestHandler<GetMemberBalanceQuery, Result<MemberBalanceDto>>
{
    private readonly PaymentsDbContext _db;

    public GetMemberBalanceQueryHandler(PaymentsDbContext db)
    {
        _db = db;
    }

    public async Task<Result<MemberBalanceDto>> Handle(GetMemberBalanceQuery request, CancellationToken ct)
    {
        var balance = await _db.MemberBalances
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.MemberId == request.MemberId, ct);

        if (balance == null)
        {
            // Return zero balance if no record exists
            return Result.Success(new MemberBalanceDto(
                Guid.Empty, request.MemberId, 0, 0, DateTime.UtcNow));
        }

        var dto = new MemberBalanceDto(
            balance.Id, balance.MemberId, balance.CreditBalance,
            balance.OutstandingBalance, balance.LastUpdated);

        return Result.Success(dto);
    }
}
