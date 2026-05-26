using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheLeague.Modules.Clubs.Domain;
using TheLeague.Modules.Clubs.Infrastructure.Persistence;
using TheLeague.Modules.Competitions.Domain;
using TheLeague.Modules.Competitions.Infrastructure.Persistence;
using TheLeague.Modules.Equipment.Domain;
using TheLeague.Modules.Equipment.Infrastructure.Persistence;
using TheLeague.Modules.Events.Domain;
using TheLeague.Modules.Events.Infrastructure.Persistence;
using TheLeague.Modules.Facilities.Domain;
using TheLeague.Modules.Facilities.Infrastructure.Persistence;
using TheLeague.Modules.Identity.Domain;
using TheLeague.Modules.Identity.Infrastructure.Persistence;
using TheLeague.Modules.Members.Domain;
using TheLeague.Modules.Members.Infrastructure.Persistence;
using TheLeague.Modules.Memberships.Domain;
using TheLeague.Modules.Memberships.Infrastructure.Persistence;
using TheLeague.Modules.Payments.Domain;
using TheLeague.Modules.Payments.Infrastructure.Persistence;
using TheLeague.Modules.Sessions.Domain;
using TheLeague.Modules.Sessions.Infrastructure.Persistence;
using TheLeague.Shared.Domain.Enums;
using TheLeague.Shared.Infrastructure.Authorization;

namespace TheLeague.Host.Seeding;

/// <summary>
/// Orchestrates database seeding in dependency order.
/// Idempotent: checks for existing data before inserting.
/// Call from Program.cs: await DatabaseSeeder.SeedAsync(app.Services);
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("🌱 Starting database seeding...");

        try
        {
            await SeedIdentityAsync(sp, logger);
            await SeedClubsAsync(sp, logger);
            await SeedMembersAsync(sp, logger);
            await SeedMembershipTypesAsync(sp, logger);
            await SeedMembershipsAsync(sp, logger);
            await SeedFacilitiesAsync(sp, logger);
            await SeedEquipmentAsync(sp, logger);
            await SeedSessionsAsync(sp, logger);
            await SeedEventsAsync(sp, logger);
            await SeedCompetitionsAsync(sp, logger);
            await SeedPaymentsAsync(sp, logger);

            var summary = SeedData.GetSummary();
            logger.LogInformation(
                "✅ Seeding complete! {Clubs} clubs, {Managers} managers, {Members} members, " +
                "{Sessions} sessions, {Events} events, {Competitions} competitions, {Payments} payments",
                summary.ClubCount, summary.ManagerCount, summary.MemberCount,
                summary.SessionCount, summary.EventCount, summary.CompetitionCount, summary.PaymentCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error during database seeding");
            throw;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // IDENTITY: SuperAdmin + ClubManagers + Roles
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task SeedIdentityAsync(IServiceProvider sp, ILogger logger)
    {
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure roles exist
        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("  Created role: {Role}", role);
            }
        }

        // Seed SuperAdmin
        await CreateUserIfNotExists(userManager, logger, SeedData.SuperAdmin);

        // Seed ClubManagers
        foreach (var manager in SeedData.ClubManagers)
        {
            await CreateUserIfNotExists(userManager, logger, manager);
        }

        logger.LogInformation("  ✓ Identity seeded: 1 SuperAdmin, {Count} managers/coaches", SeedData.ClubManagers.Count);
    }

    private static async Task CreateUserIfNotExists(
        UserManager<ApplicationUser> userManager, ILogger logger, UserSeedDto dto)
    {
        var existing = await userManager.FindByEmailAsync(dto.Email);
        if (existing != null) return;

        var user = new ApplicationUser
        {
            Id = dto.Id.ToString(),
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            ClubId = dto.ClubId,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, SeedData.DefaultPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, dto.Role);
        }
        else
        {
            logger.LogWarning("  Failed to create user {Email}: {Errors}",
                dto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CLUBS
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task SeedClubsAsync(IServiceProvider sp, ILogger logger)
    {
        var dbContext = sp.GetRequiredService<ClubsDbContext>();

        foreach (var clubDto in SeedData.Clubs)
        {
            var exists = await dbContext.Set<Club>().AnyAsync(c => c.Id == clubDto.Id);
            if (exists) continue;

            var club = Club.Create(clubDto.Name, clubDto.Slug, clubDto.ClubType);

            // Use reflection to set the deterministic Id and other properties
            SetEntityId(club, clubDto.Id);
            club.Update(clubDto.Name, null, clubDto.ContactEmail, clubDto.ContactPhone, clubDto.Address, clubDto.Website);
            club.UpdateBranding(clubDto.PrimaryColor, clubDto.SecondaryColor, clubDto.AccentColor, null);

            dbContext.Set<Club>().Add(club);
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("  ✓ Clubs seeded: {Count}", SeedData.Clubs.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MEMBERS
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task SeedMembersAsync(IServiceProvider sp, ILogger logger)
    {
        var dbContext = sp.GetRequiredService<MembersDbContext>();
        var existingCount = await dbContext.Set<Member>().CountAsync();
        if (existingCount > 0) return;

        foreach (var dto in SeedData.Members)
        {
            var member = Member.Create(dto.ClubId, dto.FirstName, dto.LastName, dto.Email);
            SetEntityId(member, dto.Id);
            SetProperty(member, "ClubId", dto.ClubId);
            member.SetMemberNumber(dto.MemberNumber);

            // Set status via transitions
            if (dto.Status == MemberStatus.Active)
                member.Activate();
            else if (dto.Status == MemberStatus.Suspended)
            {
                member.Activate();
                member.Suspend();
            }
            else if (dto.Status == MemberStatus.Expired)
            {
                member.Activate();
                member.Expire();
            }
            // Pending is the default

            // Set additional properties via reflection for seed data
            SetProperty(member, "Phone", dto.Phone);
            SetProperty(member, "DateOfBirth", dto.DateOfBirth);
            SetProperty(member, "Gender", dto.Gender);
            SetProperty(member, "JoinedDate", dto.JoinedDate);
            SetProperty(member, "EmailOptIn", true);

            dbContext.Set<Member>().Add(member);
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("  ✓ Members seeded: {Count}", SeedData.Members.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MEMBERSHIP TYPES
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task SeedMembershipTypesAsync(IServiceProvider sp, ILogger logger)
    {
        var dbContext = sp.GetRequiredService<MembershipsDbContext>();
        var existingCount = await dbContext.Set<MembershipType>().CountAsync();
        if (existingCount > 0) return;

        foreach (var dto in SeedData.MembershipTypes)
        {
            var mt = MembershipType.Create(dto.ClubId, dto.Name, dto.Price, dto.BillingCycle);
            SetEntityId(mt, dto.Id);
            SetProperty(mt, "ClubId", dto.ClubId);
            mt.Update(dto.Name, dto.Description, dto.Price, dto.BillingCycle,
                dto.MinAge, dto.MaxAge, dto.Capacity, dto.JoiningFee, true, true, null);

            dbContext.Set<MembershipType>().Add(mt);
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("  ✓ Membership types seeded: {Count}", SeedData.MembershipTypes.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MEMBERSHIPS
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task SeedMembershipsAsync(IServiceProvider sp, ILogger logger)
    {
        var dbContext = sp.GetRequiredService<MembershipsDbContext>();
        var existingCount = await dbContext.Set<Membership>().CountAsync();
        if (existingCount > 0) return;

        var memberships = SeedData.GenerateMemberships();
        foreach (var dto in memberships)
        {
            var membership = Membership.Create(
                dto.ClubId, dto.MemberId, dto.MembershipTypeId,
                dto.StartDate, dto.EndDate, dto.AutoRenew, dto.PricePaid);
            SetEntityId(membership, dto.Id);

            if (dto.Status == MembershipStatus.Expired)
                membership.Expire();

            dbContext.Set<Membership>().Add(membership);
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("  ✓ Memberships seeded: {Count}", memberships.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // FACILITIES
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task SeedFacilitiesAsync(IServiceProvider sp, ILogger logger)
    {
        var dbContext = sp.GetRequiredService<FacilitiesDbContext>();
        var existingCount = await dbContext.Set<Facility>().CountAsync();
        if (existingCount > 0) return;

        foreach (var dto in SeedData.Facilities)
        {
            var facilityType = Enum.Parse<TheLeague.Modules.Facilities.Domain.FacilityType>(dto.FacilityType);
            var facility = Facility.Create(dto.ClubId, dto.Name, facilityType, dto.Description, dto.Capacity);
            SetEntityId(facility, dto.Id);
            SetProperty(facility, "ClubId", dto.ClubId);

            dbContext.Set<Facility>().Add(facility);
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("  ✓ Facilities seeded: {Count}", SeedData.Facilities.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // EQUIPMENT
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task SeedEquipmentAsync(IServiceProvider sp, ILogger logger)
    {
        var dbContext = sp.GetRequiredService<EquipmentDbContext>();
        var existingCount = await dbContext.Set<EquipmentItem>().CountAsync();
        if (existingCount > 0) return;

        foreach (var dto in SeedData.Equipment)
        {
            var item = EquipmentItem.Create(
                dto.ClubId, dto.Name, dto.Category, dto.Condition,
                dto.Location, DateTime.UtcNow.AddYears(-2), dto.Value,
                dto.AnnualDepreciationRate, null);
            SetEntityId(item, dto.Id);
            SetProperty(item, "ClubId", dto.ClubId);

            dbContext.Set<EquipmentItem>().Add(item);
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("  ✓ Equipment seeded: {Count}", SeedData.Equipment.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SESSIONS
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task SeedSessionsAsync(IServiceProvider sp, ILogger logger)
    {
        var dbContext = sp.GetRequiredService<SessionsDbContext>();
        var existingCount = await dbContext.Set<Session>().CountAsync();
        if (existingCount > 0) return;

        var sessions = SeedData.GenerateSessions();
        foreach (var dto in sessions)
        {
            var session = Session.Create(
                dto.ClubId, dto.Title, dto.Category,
                null, null, dto.StartTime, dto.Duration,
                dto.Capacity, dto.Fee);
            SetEntityId(session, dto.Id);
            SetProperty(session, "ClubId", dto.ClubId);
            SetProperty(session, "CurrentBookingCount", dto.CurrentBookingCount);

            if (dto.IsCancelled)
                session.Cancel("Weather conditions / insufficient numbers");

            dbContext.Set<Session>().Add(session);
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("  ✓ Sessions seeded: {Count}", sessions.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // EVENTS
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task SeedEventsAsync(IServiceProvider sp, ILogger logger)
    {
        var dbContext = sp.GetRequiredService<EventsDbContext>();
        var existingCount = await dbContext.Set<Event>().CountAsync();
        if (existingCount > 0) return;

        foreach (var dto in SeedData.Events)
        {
            var evt = Event.Create(dto.ClubId, dto.Title, dto.EventType, dto.StartDateTime, dto.EndDateTime);
            SetEntityId(evt, dto.Id);
            SetProperty(evt, "ClubId", dto.ClubId);
            SetProperty(evt, "VenueName", dto.VenueName);
            SetProperty(evt, "Capacity", dto.Capacity);
            SetProperty(evt, "IsTicketed", dto.IsTicketed);
            SetProperty(evt, "StandardPrice", dto.StandardPrice);
            SetProperty(evt, "MemberPrice", dto.MemberPrice);
            SetProperty(evt, "AllowRsvp", true);

            // Transition to target status
            if (dto.Status == EventStatus.Published || dto.Status == EventStatus.RegistrationOpen)
            {
                evt.Publish();
                if (dto.Status == EventStatus.RegistrationOpen)
                    evt.OpenRegistration();
            }

            dbContext.Set<Event>().Add(evt);
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("  ✓ Events seeded: {Count}", SeedData.Events.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // COMPETITIONS (Seasons → Competitions → Teams)
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task SeedCompetitionsAsync(IServiceProvider sp, ILogger logger)
    {
        var dbContext = sp.GetRequiredService<CompetitionsDbContext>();
        var existingSeasons = await dbContext.Set<Season>().CountAsync();
        if (existingSeasons > 0) return;

        // Seasons
        foreach (var dto in SeedData.Seasons)
        {
            var season = Season.Create(dto.ClubId, dto.Name, dto.StartDate, dto.EndDate);
            SetEntityId(season, dto.Id);
            SetProperty(season, "ClubId", dto.ClubId);
            dbContext.Set<Season>().Add(season);
        }

        await dbContext.SaveChangesAsync();

        // Competitions
        foreach (var dto in SeedData.Competitions)
        {
            var competition = Competition.Create(
                dto.ClubId, dto.SeasonId, dto.Name, dto.CompetitionType,
                dto.PointsForWin, dto.PointsForDraw, dto.PointsForLoss);
            SetEntityId(competition, dto.Id);
            SetProperty(competition, "ClubId", dto.ClubId);
            competition.Activate();
            dbContext.Set<Competition>().Add(competition);
        }

        await dbContext.SaveChangesAsync();

        // Teams
        foreach (var dto in SeedData.CompetitionTeams)
        {
            var team = CompetitionTeam.Create(
                dto.ClubId, dto.CompetitionId, dto.TeamName,
                null, null, null, dto.TeamColor, dto.SquadSize);
            SetEntityId(team, dto.Id);
            SetProperty(team, "ClubId", dto.ClubId);
            dbContext.Set<CompetitionTeam>().Add(team);
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("  ✓ Competitions seeded: {Seasons} seasons, {Comps} competitions, {Teams} teams",
            SeedData.Seasons.Count, SeedData.Competitions.Count, SeedData.CompetitionTeams.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PAYMENTS
    // ─────────────────────────────────────────────────────────────────────────
    private static async Task SeedPaymentsAsync(IServiceProvider sp, ILogger logger)
    {
        var dbContext = sp.GetRequiredService<PaymentsDbContext>();
        var existingCount = await dbContext.Set<Payment>().CountAsync();
        if (existingCount > 0) return;

        var payments = SeedData.GeneratePayments();
        int batchSize = 100;
        int count = 0;

        foreach (var dto in payments)
        {
            var payment = Payment.Create(
                dto.ClubId, dto.MemberId, dto.Amount, dto.Method, dto.Type, dto.Description);
            SetEntityId(payment, dto.Id);
            SetProperty(payment, "ClubId", dto.ClubId);
            SetProperty(payment, "PaymentDate", dto.PaymentDate);

            if (dto.Status == PaymentStatus.Completed)
                payment.Complete($"txn_seed_{dto.Id.ToString()[..8]}", dto.Amount * 0.029m);
            else if (dto.Status == PaymentStatus.Failed)
                payment.Fail("Card declined - insufficient funds");

            dbContext.Set<Payment>().Add(payment);
            count++;

            // Batch saves to avoid memory pressure
            if (count % batchSize == 0)
                await dbContext.SaveChangesAsync();
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("  ✓ Payments seeded: {Count}", payments.Count);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UTILITY: Set private/protected properties via reflection for seeding
    // ─────────────────────────────────────────────────────────────────────────
    private static void SetEntityId(object entity, Guid id)
    {
        var prop = entity.GetType().GetProperty("Id");
        if (prop != null && prop.CanWrite)
        {
            prop.SetValue(entity, id);
            return;
        }

        // Try via backing field or base class
        var type = entity.GetType();
        while (type != null)
        {
            prop = type.GetProperty("Id",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            if (prop != null)
            {
                prop.SetValue(entity, id);
                return;
            }
            type = type.BaseType;
        }
    }

    private static void SetProperty<T>(object entity, string propertyName, T value)
    {
        var type = entity.GetType();
        while (type != null)
        {
            var prop = type.GetProperty(propertyName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);
            if (prop != null)
            {
                prop.SetValue(entity, value);
                return;
            }
            type = type.BaseType;
        }
    }
}
