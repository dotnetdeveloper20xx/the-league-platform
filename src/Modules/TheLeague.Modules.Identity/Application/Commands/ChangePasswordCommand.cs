using MediatR;
using Microsoft.AspNetCore.Identity;
using TheLeague.Modules.Identity.Domain;

namespace TheLeague.Modules.Identity.Application.Commands;

public record ChangePasswordCommand(string UserId, string CurrentPassword, string NewPassword) : IRequest<ChangePasswordResult>;

public record ChangePasswordResult(bool Success, IEnumerable<string>? Errors);

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordResult>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ChangePasswordCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ChangePasswordResult> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return new ChangePasswordResult(false, new[] { "User not found." });

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return new ChangePasswordResult(false, result.Errors.Select(e => e.Description));

        return new ChangePasswordResult(true, null);
    }
}
