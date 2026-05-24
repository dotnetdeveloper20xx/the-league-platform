using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheLeague.Modules.Memberships.Infrastructure.Persistence;
using TheLeague.Shared.Contracts;

namespace TheLeague.Modules.Memberships;

public class MembershipsModule : IModule
{
    public string Name => "Memberships";

    public void RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MembershipsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "memberships")));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MembershipsModule).Assembly));
    }

    public void UseModule(IApplicationBuilder app)
    {
        // No middleware needed for this module
    }
}
