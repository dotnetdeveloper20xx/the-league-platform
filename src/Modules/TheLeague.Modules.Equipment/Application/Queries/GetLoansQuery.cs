using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Equipment.Application.Dtos;
using TheLeague.Modules.Equipment.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Equipment.Application.Queries;

public record GetLoansQuery(
    LoanStatus? Status = null,
    bool? OverdueOnly = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<LoanDto>>;

public class GetLoansQueryHandler : IRequestHandler<GetLoansQuery, PagedResult<LoanDto>>
{
    private readonly EquipmentDbContext _db;

    public GetLoansQueryHandler(EquipmentDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<LoanDto>> Handle(GetLoansQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var query = _db.EquipmentLoans.AsNoTracking().AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(l => l.Status == request.Status.Value);
        }
        else if (request.OverdueOnly == true)
        {
            query = query.Where(l => l.Status == LoanStatus.Overdue ||
                (l.Status == LoanStatus.Active && l.ExpectedReturnDate < DateTime.UtcNow));
        }
        else
        {
            // Default: show active and overdue loans
            query = query.Where(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Overdue);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(l => l.LoanDate)
            .Skip((request.Page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LoanDto(
                l.Id, l.EquipmentId, l.MemberId, l.Status,
                l.LoanDate, l.ExpectedReturnDate, l.ActualReturnDate,
                l.Fee, l.Deposit, l.Notes, l.CreatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<LoanDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = pageSize
        };
    }
}
