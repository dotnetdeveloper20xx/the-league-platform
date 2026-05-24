using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheLeague.Modules.Clubs.Infrastructure.Persistence;
using TheLeague.Shared.Contracts;

namespace TheLeague.Modules.Clubs;

public class ClubsModule : IModule
{
    public string Name => "Clubs";

    public void RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ClubsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "clubs")));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ClubsModule).Assembly));
    }

    public void UseModule(IApplicationBuilder app)
    {
        // No middleware needed for this module
    }
}
