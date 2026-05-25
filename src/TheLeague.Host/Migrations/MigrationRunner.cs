using Microsoft.EntityFrameworkCore;

namespace TheLeague.Host.Migrations;

public static class MigrationRunner
{
    public static async Task RunMigrationsAsync(IServiceProvider services)
    {
        // Run migrations for all module DbContexts
        // Each module has its own schema and migration history table
        // Migrations are idempotent - safe to run multiple times

        // TODO: Iterate over all registered DbContext types and apply pending migrations
        // Example:
        // using var scope = services.CreateScope();
        // var dbContext = scope.ServiceProvider.GetRequiredService<IdentityModuleDbContext>();
        // await dbContext.Database.MigrateAsync();

        await Task.CompletedTask;
    }
}
