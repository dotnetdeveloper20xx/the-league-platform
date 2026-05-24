using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Identity.Application.Dtos;
using TheLeague.Modules.Identity.Domain;
using TheLeague.Modules.Identity.Infrastructure.Persistence;
using TheLeague.Modules.Identity.Infrastructure.Services;

namespace TheLeague.Modules.Identity.Application.Commands;

public record LoginCommand(string Email, string Password, string? DeviceIdentifier, string? IpAddress) : IRequest<LoginResult>;

public record LoginResult(bool Success, AuthResponse? Response, string? Error, int? LockoutSecondsRemaining);

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtService _jwtService;
    private readonly IdentityModuleDbContext _dbContext;

    public LoginCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtService jwtService,
        IdentityModuleDbContext dbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _dbContext = dbContext;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return new LoginResult(false, null, "Invalid credentials.", null);

        // Check lockout
        if (await _userManager.IsLockedOutAsync(user))
        {
            var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
            var remaining = lockoutEnd.HasValue
                ? (int)(lockoutEnd.Value - DateTimeOffset.UtcNow).TotalSeconds
                : 0;
            return new LoginResult(false, null, "Account is locked.", remaining > 0 ? remaining : null);
        }

        // Check email verification
        if (!await _userManager.IsEmailConfirmedAsync(user))
            return new LoginResult(false, null, "Email verification is required.", null);

        // Validate password
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                // TODO: Send lockout notification email
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var remaining = lockoutEnd.HasValue
                    ? (int)(lockoutEnd.Value - DateTimeOffset.UtcNow).TotalSeconds
                    : 0;
                return new LoginResult(false, null, "Account is locked.", remaining > 0 ? remaining : null);
            }
            return new LoginResult(false, null, "Invalid credentials.", null);
        }

        // Get role
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Member";

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user, role);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        _dbContext.RefreshTokens.Add(refreshToken);

        // Create or update session
        var session = new UserSession
        {
            UserId = user.Id,
            DeviceIdentifier = request.DeviceIdentifier ?? "Unknown",
            IpAddress = request.IpAddress
        };
        _dbContext.UserSessions.Add(session);

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        await _dbContext.SaveChangesAsync(cancellationToken);

        var userDto = new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, role, user.ClubId, user.MemberId);
        var response = new AuthResponse(accessToken, refreshTokenValue, DateTime.UtcNow.AddMinutes(15), userDto);

        return new LoginResult(true, response, null, null);
    }
}
