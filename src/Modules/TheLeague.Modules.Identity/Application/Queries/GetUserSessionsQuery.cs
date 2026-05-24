using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Identity.Application.Dtos;
using TheLeague.Modules.Identity.Infrastructure.Persistence;

namespace TheLeague.Modules.Identity.Application.Queries;

public record GetUserSessionsQuery(string UserId) : IRequest<List<SessionDto>>;

public class GetUserSessionsQueryHandler : IRequestHandler<GetUserSessionsQuery, List<SessionDto>>
{
    private readonly IdentityModuleDbContext _dbContext;

    public GetUserSessionsQueryHandler(IdentityModuleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<SessionDto>> Handle(GetUserSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _dbContext.UserSessions
            .Where(s => s.UserId == request.UserId && !s.IsRevoked)
            .OrderByDescending(s => s.LastActiveAt)
            .Select(s => new SessionDto(s.Id, s.DeviceIdentifier, s.IpAddress, s.LastActiveAt, s.CreatedAt))
            .ToListAsync(cancellationToken);

        return sessions;
    }
}
