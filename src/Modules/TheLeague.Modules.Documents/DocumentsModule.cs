using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheLeague.Modules.Documents.Infrastructure.Persistence;
using TheLeague.Modules.Documents.Infrastructure.Services;
using TheLeague.Shared.Contracts;

namespace TheLeague.Modules.Documents;

public class DocumentsModule : IModule
{
    public string Name => "Documents";

    public void RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DocumentsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "documents")));

        services.AddScoped<IBlobStorageService, MockBlobStorageService>();
        services.AddScoped<IFileValidator, FileValidator>();
        services.AddScoped<IImageOptimizer, ImageOptimizer>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DocumentsModule).Assembly));
    }

    public void UseModule(IApplicationBuilder app)
    {
        // No middleware needed for this module
    }
}
