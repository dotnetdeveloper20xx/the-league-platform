using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheLeague.Modules.Facilities.Application.Services;
using TheLeague.Modules.Facilities.Infrastructure.Persistence;
using TheLeague.Shared.Contracts;

namespace TheLeague.Modules.Facilities;

public class FacilitiesModule : IModule
{
    public string Name => "Facilities";

    public void RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FacilitiesDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "facilities")));

        services.AddScoped<ConflictDetectionService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(FacilitiesModule).Assembly));
    }

    public void UseModule(IApplicationBuilder app)
    {
        // No middleware needed for this module
    }
}
