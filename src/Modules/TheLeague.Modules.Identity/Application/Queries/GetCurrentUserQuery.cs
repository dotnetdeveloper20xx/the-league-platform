using MediatR;
using Microsoft.AspNetCore.Identity;
using TheLeague.Modules.Identity.Application.Dtos;
using TheLeague.Modules.Identity.Domain;

namespace TheLeague.Modules.Identity.Application.Queries;

public record GetCurrentUserQuery(string UserId) : IRequest<UserDto?>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto?>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetCurrentUserQueryHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserDto?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Member";

        return new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, role, user.ClubId, user.MemberId);
    }
}
