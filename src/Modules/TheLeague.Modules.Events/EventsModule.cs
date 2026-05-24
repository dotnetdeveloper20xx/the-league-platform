using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheLeague.Modules.Events.Infrastructure.Persistence;
using TheLeague.Shared.Contracts;

namespace TheLeague.Modules.Events;

public class EventsModule : IModule
{
    public string Name => "Events";

    public void RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EventsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "events")));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EventsModule).Assembly));
    }

    public void UseModule(IApplicationBuilder app)
    {
        // No middleware needed for this module
    }
}
