using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheLeague.Modules.Members.Infrastructure.Persistence;
using TheLeague.Modules.Members.Infrastructure.Services;
using TheLeague.Shared.Contracts;

namespace TheLeague.Modules.Members;

public class MembersModule : IModule
{
    public string Name => "Members";

    public void RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MembersDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "members")));

        services.AddScoped<MemberNumberGenerator>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(MembersModule).Assembly));
    }

    public void UseModule(IApplicationBuilder app)
    {
        // No middleware needed for this module
    }
}
