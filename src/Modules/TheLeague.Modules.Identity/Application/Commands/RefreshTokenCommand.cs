using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Identity.Application.Dtos;
using TheLeague.Modules.Identity.Domain;
using TheLeague.Modules.Identity.Infrastructure.Persistence;
using TheLeague.Modules.Identity.Infrastructure.Services;

namespace TheLeague.Modules.Identity.Application.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<RefreshTokenResult>;

public record RefreshTokenResult(bool Success, AuthResponse? Response, string? Error);

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtService _jwtService;
    private readonly IdentityModuleDbContext _dbContext;

    public RefreshTokenCommandHandler(
        UserManager<ApplicationUser> userManager,
        JwtService jwtService,
        IdentityModuleDbContext dbContext)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _dbContext = dbContext;
    }

    public async Task<RefreshTokenResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == request.RefreshToken, cancellationToken);

        if (storedToken is null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            // Security measure: if token is already used/revoked, revoke ALL user's tokens
            if (storedToken is not null)
            {
                var userTokens = await _dbContext.RefreshTokens
                    .Where(t => t.UserId == storedToken.UserId && !t.IsRevoked)
                    .ToListAsync(cancellationToken);

                foreach (var token in userTokens)
                {
                    token.IsRevoked = true;
                    token.RevokedReason = "Potential token reuse detected";
                }
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return new RefreshTokenResult(false, null, "Invalid or expired refresh token.");
        }

        // Invalidate consumed token
        storedToken.IsRevoked = true;
        storedToken.RevokedReason = "Consumed";

        var user = await _userManager.FindByIdAsync(storedToken.UserId);
        if (user is null)
            return new RefreshTokenResult(false, null, "User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Member";

        // Issue new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(user, role);
        var newRefreshTokenValue = _jwtService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        _dbContext.RefreshTokens.Add(newRefreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, role, user.ClubId, user.MemberId);
        var response = new AuthResponse(newAccessToken, newRefreshTokenValue, DateTime.UtcNow.AddMinutes(15), userDto);

        return new RefreshTokenResult(true, response, null);
    }
}
