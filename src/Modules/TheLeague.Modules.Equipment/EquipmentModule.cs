using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheLeague.Modules.Equipment.Infrastructure.Persistence;
using TheLeague.Shared.Contracts;

namespace TheLeague.Modules.Equipment;

public class EquipmentModule : IModule
{
    public string Name => "Equipment";

    public void RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EquipmentDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "equipment")));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(EquipmentModule).Assembly));
    }

    public void UseModule(IApplicationBuilder app)
    {
        // No middleware needed for this module
    }
}
