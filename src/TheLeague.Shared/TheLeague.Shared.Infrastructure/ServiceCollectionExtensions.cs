using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using TheLeague.Shared.Contracts.Messaging;
using TheLeague.Shared.Contracts.Services;
using TheLeague.Shared.Infrastructure.Authorization;
using TheLeague.Shared.Infrastructure.Behaviours;
using TheLeague.Shared.Infrastructure.Caching;
using TheLeague.Shared.Infrastructure.Messaging;
using TheLeague.Shared.Infrastructure.Tenancy;

namespace TheLeague.Shared.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Tenant service (scoped - per request)
        services.AddScoped<ITenantService, TenantService>();

        // Redis (optional - graceful fallback to memory cache)
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            try
            {
                IConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisConnection);
                services.AddSingleton(redis);
            }
            catch
            {
                // Redis unavailable - no IConnectionMultiplexer registered
            }
        }

        // Memory cache (always available as fallback)
        services.AddMemoryCache();

        // Cache service (resolves IConnectionMultiplexer optionally)
        services.AddSingleton<ICacheService>(sp =>
        {
            var redis = sp.GetService<IConnectionMultiplexer>();
            var memoryCache = sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
            return new RedisCacheService(redis, memoryCache);
        });

        // Integration event bus (singleton - lives for app lifetime)
        services.AddSingleton<IIntegrationEventBus, InProcessIntegrationEventBus>();

        // MediatR Pipeline Behaviours (order matters)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TenantBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehaviour<,>));

        // Authorization
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }
}
