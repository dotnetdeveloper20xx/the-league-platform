using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Programs.Application.Dtos;
using TheLeague.Modules.Programs.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Programs.Application.Queries;

public record GetProgramEnrollmentsQuery(
    Guid ProgramId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<ProgramEnrollmentDto>>;

public class GetProgramEnrollmentsQueryHandler : IRequestHandler<GetProgramEnrollmentsQuery, PagedResult<ProgramEnrollmentDto>>
{
    private readonly ProgramsDbContext _db;

    public GetProgramEnrollmentsQueryHandler(ProgramsDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<ProgramEnrollmentDto>> Handle(GetProgramEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _db.ProgramEnrollments
            .AsNoTracking()
            .Where(e => e.ProgramId == request.ProgramId)
            .OrderBy(e => e.EnrolledAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(e => new ProgramEnrollmentDto(
                e.Id, e.ProgramId, e.MemberId,
                e.Status, e.EnrolledAt, e.CompletedAt,
                e.WaitlistPosition))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProgramEnrollmentDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
