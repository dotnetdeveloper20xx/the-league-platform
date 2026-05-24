using Microsoft.AspNetCore.Authorization;

namespace TheLeague.Shared.Infrastructure.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var roleClaim = context.User.FindFirst("role")?.Value
                     ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (roleClaim is null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (RolePermissionMapping.HasPermission(roleClaim, requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
