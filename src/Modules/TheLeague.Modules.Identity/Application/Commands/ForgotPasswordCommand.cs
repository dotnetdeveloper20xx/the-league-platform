using MediatR;
using Microsoft.AspNetCore.Identity;
using TheLeague.Modules.Identity.Domain;

namespace TheLeague.Modules.Identity.Application.Commands;

public record ForgotPasswordCommand(string Email) : IRequest<ForgotPasswordResult>;

public record ForgotPasswordResult(bool Success);

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResult>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ForgotPasswordCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ForgotPasswordResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            // Don't reveal that the user doesn't exist
            return new ForgotPasswordResult(true);
        }

        // Generate reset token (60-minute expiry is configured via token provider options)
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // TODO: Send email with reset token/link
        // The token is valid for 60 minutes as configured in the token provider

        return new ForgotPasswordResult(true);
    }
}
