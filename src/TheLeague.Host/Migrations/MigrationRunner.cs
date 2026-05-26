using Microsoft.EntityFrameworkCore;
using TheLeague.Modules.Analytics.Infrastructure.Persistence;
using TheLeague.Modules.Clubs.Infrastructure.Persistence;
using TheLeague.Modules.Communications.Infrastructure.Persistence;
using TheLeague.Modules.Competitions.Infrastructure.Persistence;
using TheLeague.Modules.Documents.Infrastructure.Persistence;
using TheLeague.Modules.Equipment.Infrastructure.Persistence;
using TheLeague.Modules.Events.Infrastructure.Persistence;
using TheLeague.Modules.Facilities.Infrastructure.Persistence;
using TheLeague.Modules.Identity.Infrastructure.Persistence;
using TheLeague.Modules.Members.Infrastructure.Persistence;
using TheLeague.Modules.Memberships.Infrastructure.Persistence;
using TheLeague.Modules.Payments.Infrastructure.Persistence;
using TheLeague.Modules.Programs.Infrastructure.Persistence;
using TheLeague.Modules.Sessions.Infrastructure.Persistence;
using TheLeague.Modules.Shop.Infrastructure.Persistence;
using TheLeague.Modules.Subscriptions.Infrastructure.Persistence;

namespace TheLeague.Host.Migrations;

public static class MigrationRunner
{
    public static async Task RunMigrationsAsync(IServiceProvider services)
    {
        // Apply pending migrations for all module DbContexts
        // Each module has its own schema and migration history table
        // Migrations are idempotent - safe to run multiple times

        await MigrateAsync<IdentityModuleDbContext>(services);
        await MigrateAsync<ClubsDbContext>(services);
        await MigrateAsync<MembersDbContext>(services);
        await MigrateAsync<MembershipsDbContext>(services);
        await MigrateAsync<SessionsDbContext>(services);
        await MigrateAsync<EventsDbContext>(services);
        await MigrateAsync<CompetitionsDbContext>(services);
        await MigrateAsync<PaymentsDbContext>(services);
        await MigrateAsync<FacilitiesDbContext>(services);
        await MigrateAsync<EquipmentDbContext>(services);
        await MigrateAsync<ProgramsDbContext>(services);
        await MigrateAsync<CommunicationsDbContext>(services);
        await MigrateAsync<AnalyticsDbContext>(services);
        await MigrateAsync<ShopDbContext>(services);
        await MigrateAsync<DocumentsDbContext>(services);
        await MigrateAsync<SubscriptionsDbContext>(services);
    }

    private static async Task MigrateAsync<TContext>(IServiceProvider services)
        where TContext : DbContext
    {
        var context = services.GetRequiredService<TContext>();
        await context.Database.MigrateAsync();
    }
}
