using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheLeague.Modules.Subscriptions.Infrastructure.Persistence;
using TheLeague.Modules.Subscriptions.Infrastructure.Services;
using TheLeague.Shared.Contracts;

namespace TheLeague.Modules.Subscriptions;

public class SubscriptionsModule : IModule
{
    public string Name => "Subscriptions";

    public void RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SubscriptionsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IFeatureGateService, FeatureGateService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SubscriptionsModule).Assembly));
    }

    public void UseModule(IApplicationBuilder app) { }
}
