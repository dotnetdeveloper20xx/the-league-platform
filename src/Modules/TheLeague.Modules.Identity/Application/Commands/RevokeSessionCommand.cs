using MediatR;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Identity.Infrastructure.Persistence;

namespace TheLeague.Modules.Identity.Application.Commands;

public record RevokeSessionCommand(Guid SessionId, string UserId) : IRequest<RevokeSessionResult>;

public record RevokeSessionResult(bool Success, string? Error);

public class RevokeSessionCommandHandler : IRequestHandler<RevokeSessionCommand, RevokeSessionResult>
{
    private readonly IdentityModuleDbContext _dbContext;

    public RevokeSessionCommandHandler(IdentityModuleDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RevokeSessionResult> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
    {
        var session = await _dbContext.UserSessions
            .FirstOrDefaultAsync(s => s.Id == request.SessionId && s.UserId == request.UserId, cancellationToken);

        if (session is null)
            return new RevokeSessionResult(false, "Session not found.");

        session.IsRevoked = true;

        // Invalidate all refresh tokens for this user associated with this session
        var userTokens = await _dbContext.RefreshTokens
            .Where(t => t.UserId == request.UserId && !t.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in userTokens)
        {
            token.IsRevoked = true;
            token.RevokedReason = "Session revoked";
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RevokeSessionResult(true, null);
    }
}
