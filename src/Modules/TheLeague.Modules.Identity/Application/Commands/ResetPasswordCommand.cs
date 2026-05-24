using MediatR;
using Microsoft.AspNetCore.Identity;
using TheLeague.Modules.Identity.Domain;

namespace TheLeague.Modules.Identity.Application.Commands;

public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<ResetPasswordResult>;

public record ResetPasswordResult(bool Success, IEnumerable<string>? Errors);

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResult>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ResetPasswordCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ResetPasswordResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return new ResetPasswordResult(false, new[] { "Invalid request." });

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            return new ResetPasswordResult(false, result.Errors.Select(e => e.Description));

        return new ResetPasswordResult(true, null);
    }
}
