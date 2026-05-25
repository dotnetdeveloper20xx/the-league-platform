using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheLeague.Modules.Analytics.Infrastructure.Persistence;
using TheLeague.Modules.Analytics.Infrastructure.Services;
using TheLeague.Shared.Contracts;

namespace TheLeague.Modules.Analytics;

public class AnalyticsModule : IModule
{
    public string Name => "Analytics";

    public void RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AnalyticsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "analytics")));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AnalyticsModule).Assembly));

        // Register services
        services.AddScoped<IHealthScoreCalculator, HealthScoreCalculator>();
        services.AddScoped<IChurnPredictor, ChurnPredictor>();
        services.AddScoped<IRevenueForecaster, RevenueForecaster>();
    }

    public void UseModule(IApplicationBuilder app)
    {
        // No middleware needed for this module
    }
}
