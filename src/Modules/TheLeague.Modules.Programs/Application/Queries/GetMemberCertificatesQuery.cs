using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Programs.Application.Dtos;
using TheLeague.Modules.Programs.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Programs.Application.Queries;

public record GetMemberCertificatesQuery(
    Guid MemberId,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<MemberCertificateDto>>;

public class GetMemberCertificatesQueryHandler : IRequestHandler<GetMemberCertificatesQuery, PagedResult<MemberCertificateDto>>
{
    private readonly ProgramsDbContext _db;

    public GetMemberCertificatesQueryHandler(ProgramsDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<MemberCertificateDto>> Handle(GetMemberCertificatesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.MemberCertificates
            .AsNoTracking()
            .Where(c => c.MemberId == request.MemberId)
            .OrderByDescending(c => c.CompletionDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new MemberCertificateDto(
                c.Id, c.MemberId, c.ProgramId,
                c.ProgramName, c.SkillLevel,
                c.CompletionDate, c.CertificateNumber))
            .ToListAsync(cancellationToken);

        return new PagedResult<MemberCertificateDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
