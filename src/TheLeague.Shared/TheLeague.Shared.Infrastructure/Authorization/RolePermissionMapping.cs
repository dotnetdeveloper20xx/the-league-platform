namespace TheLeague.Shared.Infrastructure.Authorization;

public static class RolePermissionMapping
{
    private static readonly Dictionary<string, HashSet<string>> _rolePermissions = new()
    {
        [Roles.SuperAdmin] = new(new[]
        {
            Permissions.ManagePlatform, Permissions.ViewAllClubs, Permissions.ManageClub,
            Permissions.ViewClub, Permissions.ManageMembers, Permissions.ViewMembers,
            Permissions.ManageSessions, Permissions.ViewSessions, Permissions.ManageEvents,
            Permissions.ViewEvents, Permissions.ManagePayments, Permissions.ManageCompetitions,
            Permissions.ViewCompetitions, Permissions.ManageFacilities, Permissions.ManageEquipment,
            Permissions.ManagePrograms, Permissions.ManageCommunications, Permissions.ViewReports,
            Permissions.ViewPlatformReports, Permissions.ManageSettings
        }),
        [Roles.ClubManager] = new(new[]
        {
            Permissions.ManageClub, Permissions.ViewClub, Permissions.ManageMembers,
            Permissions.ViewMembers, Permissions.ManageSessions, Permissions.ViewSessions,
            Permissions.ManageEvents, Permissions.ViewEvents, Permissions.ManagePayments,
            Permissions.ManageCompetitions, Permissions.ViewCompetitions, Permissions.ManageFacilities,
            Permissions.ManageEquipment, Permissions.ManagePrograms, Permissions.ManageCommunications,
            Permissions.ViewReports, Permissions.ManageSettings
        }),
        [Roles.Member] = new(new[]
        {
            Permissions.ViewClub, Permissions.ViewSessions, Permissions.BookSessions,
            Permissions.ViewEvents, Permissions.RegisterEvents, Permissions.ViewOwnPayments,
            Permissions.ViewCompetitions, Permissions.BookFacilities, Permissions.LoanEquipment,
            Permissions.EnrollPrograms
        }),
        [Roles.Coach] = new(new[]
        {
            Permissions.ViewClub, Permissions.ManageSessions, Permissions.ViewSessions,
            Permissions.ViewMembers
        }),
        [Roles.Staff] = new(new[]
        {
            Permissions.ViewClub, Permissions.ViewMembers, Permissions.ViewSessions,
            Permissions.ViewEvents, Permissions.ViewCompetitions
        })
    };

    public static bool HasPermission(string role, string permission)
    {
        return _rolePermissions.TryGetValue(role, out var permissions) && permissions.Contains(permission);
    }

    public static IReadOnlySet<string> GetPermissions(string role)
    {
        return _rolePermissions.TryGetValue(role, out var permissions) ? permissions : new HashSet<string>();
    }
}
