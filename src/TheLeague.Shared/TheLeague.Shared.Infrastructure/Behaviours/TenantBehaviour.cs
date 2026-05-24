using MediatR;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Infrastructure.Exceptions;

namespace TheLeague.Shared.Infrastructure.Behaviours;

/// <summary>
/// Marker interface for requests that require tenant context.
/// </summary>
public interface ITenantRequest
{
    Guid ClubId { get; }
}

public class TenantBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ITenantService _tenantService;

    public TenantBehaviour(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is ITenantRequest tenantRequest)
        {
            if (tenantRequest.ClubId == Guid.Empty)
                throw new ForbiddenException("Missing or invalid tenant context.");

            // Validate that the request's ClubId matches the authenticated tenant
            if (_tenantService.CurrentTenantId.HasValue && _tenantService.CurrentTenantId.Value != tenantRequest.ClubId)
                throw new ForbiddenException("Insufficient tenant access.");
        }

        return await next();
    }
}
