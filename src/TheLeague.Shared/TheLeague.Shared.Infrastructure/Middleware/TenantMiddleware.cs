using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using TheLeague.Shared.Contracts.Services;

namespace TheLeague.Shared.Infrastructure.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var clubIdClaim = context.User.FindFirst("clubId")?.Value;
            if (!string.IsNullOrEmpty(clubIdClaim) && Guid.TryParse(clubIdClaim, out var clubId))
            {
                tenantService.SetCurrentTenant(clubId);
            }
        }

        // Fallback: X-Tenant-Id header (for admin scenarios)
        if (!tenantService.CurrentTenantId.HasValue)
        {
            var headerValue = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            if (!string.IsNullOrEmpty(headerValue) && Guid.TryParse(headerValue, out var headerClubId))
            {
                tenantService.SetCurrentTenant(headerClubId);
            }
        }

        await _next(context);
    }
}

public static class TenantMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantMiddleware>();
    }
}
