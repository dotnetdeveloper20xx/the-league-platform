using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheLeague.Modules.Competitions.Infrastructure.Persistence;
using TheLeague.Modules.Competitions.Infrastructure.Services;
using TheLeague.Shared.Contracts;

namespace TheLeague.Modules.Competitions;

public class CompetitionsModule : IModule
{
    public string Name => "Competitions";

    public void RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CompetitionsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "competitions")));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CompetitionsModule).Assembly));

        services.AddScoped<FixtureGenerator>();
        services.AddScoped<StandingsCalculator>();
    }

    public void UseModule(IApplicationBuilder app)
    {
        // No middleware needed for this module
    }
}
