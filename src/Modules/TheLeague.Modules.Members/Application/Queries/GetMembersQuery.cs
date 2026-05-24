using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Members.Application.Dtos;
using TheLeague.Modules.Members.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Domain.Results;

namespace TheLeague.Modules.Members.Application.Queries;

public record GetMembersQuery(
    string? SearchTerm = null,
    MemberStatus? Status = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<PagedResult<MemberListDto>>;

public class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, PagedResult<MemberListDto>>
{
    private readonly MembersDbContext _db;

    public GetMembersQueryHandler(MembersDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<MemberListDto>> Handle(GetMembersQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Min(request.PageSize, 100);
        if (pageSize < 1) pageSize = 20;

        var query = _db.Members.AsNoTracking().AsQueryable();

        // Search by name or email
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(m =>
                m.FirstName.ToLower().Contains(term) ||
                m.LastName.ToLower().Contains(term) ||
                m.Email.ToLower().Contains(term));
        }

        // Filter by status
        if (request.Status.HasValue)
        {
            query = query.Where(m => m.Status == request.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(m => m.LastName).ThenBy(m => m.FirstName)
            .Skip((request.Page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MemberListDto(
                m.Id, m.MemberNumber, m.FirstName, m.LastName,
                m.Email, m.Phone, m.Status, m.JoinedDate, m.IsActive
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<MemberListDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = pageSize
        };
    }
}
