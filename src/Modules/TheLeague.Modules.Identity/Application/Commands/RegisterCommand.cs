using MediatR;
using Microsoft.AspNetCore.Identity;
using TheLeague.Modules.Identity.Domain;

namespace TheLeague.Modules.Identity.Application.Commands;

public record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    Guid? ClubId) : IRequest<RegisterResult>;

public record RegisterResult(bool Success, string? UserId, IEnumerable<string>? Errors);

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public RegisterCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            ClubId = request.ClubId
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return new RegisterResult(false, null, result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, "Member");

        // TODO: Generate email verification token and send verification email
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        // TODO: Send email with token/link (24-hour validity is handled by Identity token providers)

        return new RegisterResult(true, user.Id, null);
    }
}
