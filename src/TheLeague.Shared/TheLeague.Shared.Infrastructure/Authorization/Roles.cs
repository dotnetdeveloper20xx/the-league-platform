namespace TheLeague.Shared.Infrastructure.Authorization;

public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string ClubManager = "ClubManager";
    public const string Member = "Member";
    public const string Coach = "Coach";
    public const string Staff = "Staff";

    public static readonly string[] All = { SuperAdmin, ClubManager, Member, Coach, Staff };
}
