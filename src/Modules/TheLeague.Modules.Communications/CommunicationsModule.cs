using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheLeague.Modules.Communications.Infrastructure.Persistence;
using TheLeague.Modules.Communications.Infrastructure.Providers;
using TheLeague.Modules.Communications.Infrastructure.Services;
using TheLeague.Shared.Contracts;

namespace TheLeague.Modules.Communications;

public class CommunicationsModule : IModule
{
    public string Name => "Communications";

    public void RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CommunicationsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "communications")));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CommunicationsModule).Assembly));

        // Register providers (mock implementations for now)
        services.AddScoped<IEmailProvider, MockEmailProvider>();
        services.AddScoped<ISmsProvider, MockSmsProvider>();

        // Register template engine
        services.AddSingleton<TemplateEngine>();
    }

    public void UseModule(IApplicationBuilder app)
    {
        // No middleware needed for this module
    }
}
