using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheLeague.Modules.Sessions.Infrastructure.Persistence;
using TheLeague.Shared.Contracts;

namespace TheLeague.Modules.Sessions;

public class SessionsModule : IModule
{
    public string Name => "Sessions";

    public void RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SessionsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "sessions")));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SessionsModule).Assembly));
    }

    public void UseModule(IApplicationBuilder app)
    {
        // No middleware needed for this module
    }
}
