namespace TheLeague.Shared.Infrastructure.Authorization;

public static class Permissions
{
    // Platform-wide
    public const string ManagePlatform = "Platform.Manage";
    public const string ViewAllClubs = "Platform.ViewAllClubs";

    // Club management
    public const string ManageClub = "Club.Manage";
    public const string ViewClub = "Club.View";

    // Member management
    public const string ManageMembers = "Members.Manage";
    public const string ViewMembers = "Members.View";

    // Session management
    public const string ManageSessions = "Sessions.Manage";
    public const string ViewSessions = "Sessions.View";
    public const string BookSessions = "Sessions.Book";

    // Event management
    public const string ManageEvents = "Events.Manage";
    public const string ViewEvents = "Events.View";
    public const string RegisterEvents = "Events.Register";

    // Payment management
    public const string ManagePayments = "Payments.Manage";
    public const string ViewOwnPayments = "Payments.ViewOwn";

    // Competition management
    public const string ManageCompetitions = "Competitions.Manage";
    public const string ViewCompetitions = "Competitions.View";

    // Facility management
    public const string ManageFacilities = "Facilities.Manage";
    public const string BookFacilities = "Facilities.Book";

    // Equipment management
    public const string ManageEquipment = "Equipment.Manage";
    public const string LoanEquipment = "Equipment.Loan";

    // Program management
    public const string ManagePrograms = "Programs.Manage";
    public const string EnrollPrograms = "Programs.Enroll";

    // Communication
    public const string ManageCommunications = "Communications.Manage";

    // Reports
    public const string ViewReports = "Reports.View";
    public const string ViewPlatformReports = "Reports.ViewPlatform";

    // Settings
    public const string ManageSettings = "Settings.Manage";
}
