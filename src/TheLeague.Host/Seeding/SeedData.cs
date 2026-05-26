using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Host.Seeding;

/// <summary>
/// Contains all demo seed data for The League Platform.
/// Data is organized by dependency order: Clubs → Users → Members → Memberships → Sessions → Events → Competitions → Payments → Facilities → Equipment.
/// All GUIDs are deterministic to allow idempotent seeding and cross-module references.
/// </summary>
public static class SeedData
{
    // ─────────────────────────────────────────────────────────────────────────
    // DEMO CREDENTIALS
    // ─────────────────────────────────────────────────────────────────────────
    public const string DefaultPassword = "Demo123!";

    // ─────────────────────────────────────────────────────────────────────────
    // CLUB IDs (deterministic)
    // ─────────────────────────────────────────────────────────────────────────
    public static readonly Guid TeddingtonCricketClubId = new("10000000-0000-0000-0000-000000000001");
    public static readonly Guid HighburyUnitedFCId = new("10000000-0000-0000-0000-000000000002");
    public static readonly Guid RichmondHockeyClubId = new("10000000-0000-0000-0000-000000000003");
    public static readonly Guid MaryleboneCricketClubId = new("10000000-0000-0000-0000-000000000004");

    // ─────────────────────────────────────────────────────────────────────────
    // USER IDs
    // ─────────────────────────────────────────────────────────────────────────
    public static readonly Guid SuperAdminUserId = new("20000000-0000-0000-0000-000000000001");

    // ─────────────────────────────────────────────────────────────────────────
    // CLUBS
    // ─────────────────────────────────────────────────────────────────────────
    public static readonly IReadOnlyList<ClubSeedDto> Clubs = new List<ClubSeedDto>
    {
        new(TeddingtonCricketClubId, "Teddington Cricket Club", "teddington-cricket-club", ClubType.Cricket,
            "#1B5E20", "#4CAF50", "#A5D6A7",
            "info@teddingtoncc.co.uk", "+44 20 8977 1234", "Bushy Park, Teddington TW11 0JX",
            "https://www.teddingtoncc.co.uk"),
        new(HighburyUnitedFCId, "Highbury United FC", "highbury-united-fc", ClubType.Football,
            "#B71C1C", "#F44336", "#FFCDD2",
            "secretary@highburyunited.co.uk", "+44 20 7226 5678", "Highbury Fields, London N5 1AR",
            "https://www.highburyunited.co.uk"),
        new(RichmondHockeyClubId, "Richmond Hockey Club", "richmond-hockey-club", ClubType.Hockey,
            "#0D47A1", "#2196F3", "#BBDEFB",
            "admin@richmondhockey.co.uk", "+44 20 8940 9012", "Old Deer Park, Richmond TW9 2SF",
            "https://www.richmondhockey.co.uk"),
        new(MaryleboneCricketClubId, "Marylebone Cricket Club", "marylebone-cricket-club", ClubType.Cricket,
            "#E65100", "#FF9800", "#FFE0B2",
            "membership@mcc.org.uk", "+44 20 7616 8500", "Lord's Ground, St John's Wood NW8 8QN",
            "https://www.lords.org"),
    };

    // ─────────────────────────────────────────────────────────────────────────
    // SUPER ADMIN
    // ─────────────────────────────────────────────────────────────────────────
    public static readonly UserSeedDto SuperAdmin = new(
        SuperAdminUserId, "admin@theleague.com", "Platform", "Admin", null, "SuperAdmin");

    // ─────────────────────────────────────────────────────────────────────────
    // CLUB MANAGERS (2-3 per club)
    // ─────────────────────────────────────────────────────────────────────────
    public static readonly IReadOnlyList<UserSeedDto> ClubManagers = new List<UserSeedDto>
    {
        // Teddington Cricket Club managers
        new(new("30000000-0000-0000-0001-000000000001"), "james.wilson@teddingtoncc.co.uk", "James", "Wilson", TeddingtonCricketClubId, "ClubManager"),
        new(new("30000000-0000-0000-0001-000000000002"), "sarah.mitchell@teddingtoncc.co.uk", "Sarah", "Mitchell", TeddingtonCricketClubId, "ClubManager"),
        new(new("30000000-0000-0000-0001-000000000003"), "david.thompson@teddingtoncc.co.uk", "David", "Thompson", TeddingtonCricketClubId, "Coach"),

        // Highbury United FC managers
        new(new("30000000-0000-0000-0002-000000000001"), "michael.brown@highburyunited.co.uk", "Michael", "Brown", HighburyUnitedFCId, "ClubManager"),
        new(new("30000000-0000-0000-0002-000000000002"), "emma.davis@highburyunited.co.uk", "Emma", "Davis", HighburyUnitedFCId, "ClubManager"),

        // Richmond Hockey Club managers
        new(new("30000000-0000-0000-0003-000000000001"), "robert.taylor@richmondhockey.co.uk", "Robert", "Taylor", RichmondHockeyClubId, "ClubManager"),
        new(new("30000000-0000-0000-0003-000000000002"), "lisa.anderson@richmondhockey.co.uk", "Lisa", "Anderson", RichmondHockeyClubId, "ClubManager"),
        new(new("30000000-0000-0000-0003-000000000003"), "mark.harris@richmondhockey.co.uk", "Mark", "Harris", RichmondHockeyClubId, "Coach"),

        // Marylebone Cricket Club managers
        new(new("30000000-0000-0000-0004-000000000001"), "william.clark@mcc.org.uk", "William", "Clark", MaryleboneCricketClubId, "ClubManager"),
        new(new("30000000-0000-0000-0004-000000000002"), "charlotte.lewis@mcc.org.uk", "Charlotte", "Lewis", MaryleboneCricketClubId, "ClubManager"),
        new(new("30000000-0000-0000-0004-000000000003"), "henry.walker@mcc.org.uk", "Henry", "Walker", MaryleboneCricketClubId, "Coach"),
    };

    // ─────────────────────────────────────────────────────────────────────────
    // MEMBERS (15-20 per club, 70% Active, 10% Pending, 10% Expired, 10% Suspended)
    // ─────────────────────────────────────────────────────────────────────────
    public static readonly IReadOnlyList<MemberSeedDto> Members = GenerateMembers();

    private static List<MemberSeedDto> GenerateMembers()
    {
        var members = new List<MemberSeedDto>();
        var random = new Random(42); // Deterministic seed

        var clubConfigs = new[]
        {
            (ClubId: TeddingtonCricketClubId, Prefix: "TCC", Count: 18),
            (ClubId: HighburyUnitedFCId, Prefix: "HUF", Count: 20),
            (ClubId: RichmondHockeyClubId, Prefix: "RHC", Count: 16),
            (ClubId: MaryleboneCricketClubId, Prefix: "MCC", Count: 17),
        };

        var firstNames = new[]
        {
            "Oliver", "George", "Harry", "Jack", "Jacob", "Noah", "Charlie", "Thomas",
            "Oscar", "William", "Amelia", "Olivia", "Emily", "Isla", "Ava", "Jessica",
            "Ella", "Mia", "Grace", "Sophie", "Liam", "Alexander", "Benjamin", "Daniel",
            "Ethan", "Freya", "Hannah", "Isabella", "Katie", "Lucy"
        };

        var lastNames = new[]
        {
            "Smith", "Jones", "Williams", "Taylor", "Brown", "Davies", "Evans", "Wilson",
            "Thomas", "Roberts", "Johnson", "Walker", "Wright", "Robinson", "Thompson",
            "White", "Hughes", "Edwards", "Green", "Hall", "Lewis", "Harris", "Clarke",
            "Patel", "Jackson", "Wood", "Turner", "Martin", "Cooper", "Hill"
        };

        int memberIndex = 0;
        foreach (var config in clubConfigs)
        {
            for (int i = 0; i < config.Count; i++)
            {
                memberIndex++;
                var firstName = firstNames[(memberIndex * 7 + i * 3) % firstNames.Length];
                var lastName = lastNames[(memberIndex * 11 + i * 5) % lastNames.Length];
                var email = $"{firstName.ToLower()}.{lastName.ToLower()}{memberIndex}@example.com";
                var memberNumber = $"{config.Prefix}-{memberIndex:D4}";

                // Status distribution: 70% Active, 10% Pending, 10% Expired, 10% Suspended
                MemberStatus status;
                var statusRoll = (memberIndex + i) % 10;
                if (statusRoll < 7) status = MemberStatus.Active;
                else if (statusRoll == 7) status = MemberStatus.Pending;
                else if (statusRoll == 8) status = MemberStatus.Expired;
                else status = MemberStatus.Suspended;

                var joinedDate = DateTime.UtcNow.AddMonths(-random.Next(1, 36)).AddDays(-random.Next(0, 28));
                var dob = new DateTime(1970 + random.Next(0, 40), random.Next(1, 13), random.Next(1, 28));
                var gender = (memberIndex % 3 == 0) ? Gender.Female : Gender.Male;
                var phone = $"+44 7{random.Next(100, 999)} {random.Next(100, 999)} {random.Next(1000, 9999)}";

                var memberId = new Guid($"40{memberIndex:D6}-0000-0000-0000-{memberIndex:D12}");

                members.Add(new MemberSeedDto(
                    memberId, config.ClubId, memberNumber, firstName, lastName, email,
                    phone, dob, gender, status, joinedDate));
            }
        }

        return members;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MEMBERSHIP TYPES (3-4 per club)
    // ─────────────────────────────────────────────────────────────────────────
    public static readonly IReadOnlyList<MembershipTypeSeedDto> MembershipTypes = new List<MembershipTypeSeedDto>
    {
        // Teddington Cricket Club
        new(new("50000000-0000-0001-0001-000000000001"), TeddingtonCricketClubId, "Adult Full Member", "Full playing and social membership for adults 18+", 285.00m, BillingCycle.Annual, 18, null, null, 50.00m),
        new(new("50000000-0000-0001-0002-000000000001"), TeddingtonCricketClubId, "Junior Member", "Playing membership for under 18s", 95.00m, BillingCycle.Annual, null, 17, null, 0m),
        new(new("50000000-0000-0001-0003-000000000001"), TeddingtonCricketClubId, "Social Member", "Non-playing social membership with clubhouse access", 75.00m, BillingCycle.Annual, 18, null, null, 25.00m),
        new(new("50000000-0000-0001-0004-000000000001"), TeddingtonCricketClubId, "Family Membership", "Two adults and up to 3 juniors", 550.00m, BillingCycle.Annual, null, null, 20, 75.00m),

        // Highbury United FC
        new(new("50000000-0000-0002-0001-000000000001"), HighburyUnitedFCId, "Senior Player", "Full playing membership for adults", 180.00m, BillingCycle.Annual, 18, null, null, 30.00m),
        new(new("50000000-0000-0002-0002-000000000001"), HighburyUnitedFCId, "Youth Player", "Playing membership for U18s", 60.00m, BillingCycle.Annual, null, 17, null, 0m),
        new(new("50000000-0000-0002-0003-000000000001"), HighburyUnitedFCId, "Monthly Fitness", "Gym and training access, monthly rolling", 35.00m, BillingCycle.Monthly, 16, null, 50, 0m),

        // Richmond Hockey Club
        new(new("50000000-0000-0003-0001-000000000001"), RichmondHockeyClubId, "Full Playing Member", "Full playing rights for all teams", 320.00m, BillingCycle.Annual, 18, null, null, 60.00m),
        new(new("50000000-0000-0003-0002-000000000001"), RichmondHockeyClubId, "Student Member", "Discounted rate for full-time students", 160.00m, BillingCycle.Annual, 16, 25, null, 30.00m),
        new(new("50000000-0000-0003-0003-000000000001"), RichmondHockeyClubId, "Junior Member", "U16 playing membership", 85.00m, BillingCycle.Annual, null, 15, null, 0m),
        new(new("50000000-0000-0003-0004-000000000001"), RichmondHockeyClubId, "Social Member", "Clubhouse and social events only", 50.00m, BillingCycle.Annual, 18, null, null, 0m),

        // Marylebone Cricket Club
        new(new("50000000-0000-0004-0001-000000000001"), MaryleboneCricketClubId, "Full Member", "Full MCC membership with Lord's access", 550.00m, BillingCycle.Annual, 18, null, 200, 100.00m),
        new(new("50000000-0000-0004-0002-000000000001"), MaryleboneCricketClubId, "Associate Member", "Limited match day access", 250.00m, BillingCycle.Annual, 18, null, null, 50.00m),
        new(new("50000000-0000-0004-0003-000000000001"), MaryleboneCricketClubId, "Young Cricketer", "U25 playing membership", 120.00m, BillingCycle.Annual, 16, 24, 50, 25.00m),
        new(new("50000000-0000-0004-0004-000000000001"), MaryleboneCricketClubId, "Life Member", "Lifetime membership", 8500.00m, BillingCycle.Lifetime, 18, null, 10, 0m),
    };

    // ─────────────────────────────────────────────────────────────────────────
    // MEMBERSHIPS (active memberships for active members)
    // ─────────────────────────────────────────────────────────────────────────
    public static IReadOnlyList<MembershipSeedDto> GenerateMemberships()
    {
        var memberships = new List<MembershipSeedDto>();
        var random = new Random(99);
        int idx = 0;

        foreach (var member in Members)
        {
            if (member.Status != MemberStatus.Active && member.Status != MemberStatus.Expired)
                continue;

            // Find membership types for this club
            var clubTypes = MembershipTypes.Where(mt => mt.ClubId == member.ClubId).ToList();
            if (clubTypes.Count == 0) continue;

            var selectedType = clubTypes[idx % clubTypes.Count];
            var startDate = member.JoinedDate;
            var endDate = startDate.AddYears(1);
            var status = member.Status == MemberStatus.Active
                ? MembershipStatus.Active
                : MembershipStatus.Expired;

            var membershipId = new Guid($"60000000-0000-0000-{idx:D4}-{(idx + 1):D12}");

            memberships.Add(new MembershipSeedDto(
                membershipId, member.ClubId, member.Id, selectedType.Id,
                startDate, endDate, status, selectedType.Price, true));

            idx++;
        }

        return memberships;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // FACILITIES (per club)
    // ─────────────────────────────────────────────────────────────────────────
    public static readonly IReadOnlyList<FacilitySeedDto> Facilities = new List<FacilitySeedDto>
    {
        // Teddington Cricket Club
        new(new("70000000-0000-0001-0001-000000000001"), TeddingtonCricketClubId, "Main Cricket Ground", "Field", "Full-size cricket ground with pavilion view", 200),
        new(new("70000000-0000-0001-0002-000000000001"), TeddingtonCricketClubId, "Indoor Nets", "Studio", "4-lane indoor cricket nets", 16),
        new(new("70000000-0000-0001-0003-000000000001"), TeddingtonCricketClubId, "Clubhouse", "ClubHouse", "Main clubhouse with bar and function room", 120),
        new(new("70000000-0000-0001-0004-000000000001"), TeddingtonCricketClubId, "Practice Nets (Outdoor)", "Field", "6 outdoor practice nets", 24),

        // Highbury United FC
        new(new("70000000-0000-0002-0001-000000000001"), HighburyUnitedFCId, "Main Pitch", "Field", "Full-size 11-a-side football pitch", 300),
        new(new("70000000-0000-0002-0002-000000000001"), HighburyUnitedFCId, "Training Pitch", "Field", "Half-size training pitch with floodlights", 50),
        new(new("70000000-0000-0002-0003-000000000001"), HighburyUnitedFCId, "Gym", "Gym", "Fully equipped gym with free weights and machines", 30),
        new(new("70000000-0000-0002-0004-000000000001"), HighburyUnitedFCId, "Changing Rooms", "ChangingRoom", "Home and away changing facilities", 40),

        // Richmond Hockey Club
        new(new("70000000-0000-0003-0001-000000000001"), RichmondHockeyClubId, "Astroturf Pitch 1", "Field", "Full-size water-based hockey pitch", 200),
        new(new("70000000-0000-0003-0002-000000000001"), RichmondHockeyClubId, "Astroturf Pitch 2", "Field", "Sand-based training pitch", 100),
        new(new("70000000-0000-0003-0003-000000000001"), RichmondHockeyClubId, "Clubhouse & Bar", "ClubHouse", "Two-storey clubhouse with viewing balcony", 150),
        new(new("70000000-0000-0003-0004-000000000001"), RichmondHockeyClubId, "Meeting Room", "MeetingRoom", "Committee meeting room", 20),

        // Marylebone Cricket Club
        new(new("70000000-0000-0004-0001-000000000001"), MaryleboneCricketClubId, "Lord's Main Ground", "Field", "The Home of Cricket - main playing area", 30000),
        new(new("70000000-0000-0004-0002-000000000001"), MaryleboneCricketClubId, "Nursery Ground", "Field", "Secondary ground for practice and minor matches", 500),
        new(new("70000000-0000-0004-0003-000000000001"), MaryleboneCricketClubId, "Indoor School", "Studio", "MCC Indoor Cricket School with 12 lanes", 48),
        new(new("70000000-0000-0004-0004-000000000001"), MaryleboneCricketClubId, "Long Room", "ClubHouse", "Historic Long Room in the Pavilion", 200),
    };

    // ─────────────────────────────────────────────────────────────────────────
    // EQUIPMENT (per club)
    // ─────────────────────────────────────────────────────────────────────────
    public static readonly IReadOnlyList<EquipmentSeedDto> Equipment = new List<EquipmentSeedDto>
    {
        // Teddington Cricket Club
        new(new("71000000-0000-0001-0001-000000000001"), TeddingtonCricketClubId, "Bowling Machine", EquipmentCategory.Training, EquipmentCondition.Good, "Indoor Nets", 2800.00m, 15),
        new(new("71000000-0000-0001-0002-000000000001"), TeddingtonCricketClubId, "Sight Screens (Pair)", EquipmentCategory.Sports, EquipmentCondition.Fair, "Main Ground", 4500.00m, 10),
        new(new("71000000-0000-0001-0003-000000000001"), TeddingtonCricketClubId, "Heavy Roller", EquipmentCategory.Tools, EquipmentCondition.Good, "Groundsman Shed", 6000.00m, 8),
        new(new("71000000-0000-0001-0004-000000000001"), TeddingtonCricketClubId, "PA System", EquipmentCategory.Audio, EquipmentCondition.Excellent, "Clubhouse", 1200.00m, 20),
        new(new("71000000-0000-0001-0005-000000000001"), TeddingtonCricketClubId, "First Aid Kit (Match Day)", EquipmentCategory.Medical, EquipmentCondition.New, "Pavilion", 150.00m, 0),

        // Highbury United FC
        new(new("71000000-0000-0002-0001-000000000001"), HighburyUnitedFCId, "Goal Posts (Full Size)", EquipmentCategory.Sports, EquipmentCondition.Good, "Main Pitch", 3200.00m, 10),
        new(new("71000000-0000-0002-0002-000000000001"), HighburyUnitedFCId, "Training Cones (Set of 50)", EquipmentCategory.Training, EquipmentCondition.Good, "Equipment Store", 85.00m, 25),
        new(new("71000000-0000-0002-0003-000000000001"), HighburyUnitedFCId, "Portable Floodlights", EquipmentCategory.Electronics, EquipmentCondition.Excellent, "Training Pitch", 4500.00m, 12),
        new(new("71000000-0000-0002-0004-000000000001"), HighburyUnitedFCId, "Defibrillator", EquipmentCategory.Medical, EquipmentCondition.New, "Changing Rooms", 1800.00m, 0),
        new(new("71000000-0000-0002-0005-000000000001"), HighburyUnitedFCId, "Video Analysis Camera", EquipmentCategory.Electronics, EquipmentCondition.Excellent, "Office", 950.00m, 20),

        // Richmond Hockey Club
        new(new("71000000-0000-0003-0001-000000000001"), RichmondHockeyClubId, "Goal Cages (Pair)", EquipmentCategory.Sports, EquipmentCondition.Good, "Astroturf Pitch 1", 2400.00m, 10),
        new(new("71000000-0000-0003-0002-000000000001"), RichmondHockeyClubId, "Drag Flick Ramp", EquipmentCategory.Training, EquipmentCondition.Fair, "Training Area", 800.00m, 15),
        new(new("71000000-0000-0003-0003-000000000001"), RichmondHockeyClubId, "Scoreboard (Electronic)", EquipmentCategory.Electronics, EquipmentCondition.Good, "Pitch 1 Sideline", 3500.00m, 12),
        new(new("71000000-0000-0003-0004-000000000001"), RichmondHockeyClubId, "Tackle Bags (Set of 10)", EquipmentCategory.Training, EquipmentCondition.Good, "Equipment Store", 450.00m, 20),

        // Marylebone Cricket Club
        new(new("71000000-0000-0004-0001-000000000001"), MaryleboneCricketClubId, "Bowling Machine (Pro)", EquipmentCategory.Training, EquipmentCondition.Excellent, "Indoor School", 8500.00m, 12),
        new(new("71000000-0000-0004-0002-000000000001"), MaryleboneCricketClubId, "Speed Gun", EquipmentCategory.Electronics, EquipmentCondition.New, "Indoor School", 2200.00m, 15),
        new(new("71000000-0000-0004-0003-000000000001"), MaryleboneCricketClubId, "Super Sopper", EquipmentCategory.Tools, EquipmentCondition.Good, "Nursery Ground", 12000.00m, 8),
        new(new("71000000-0000-0004-0004-000000000001"), MaryleboneCricketClubId, "Covers (Full Set)", EquipmentCategory.Sports, EquipmentCondition.Good, "Ground Store", 15000.00m, 10),
        new(new("71000000-0000-0004-0005-000000000001"), MaryleboneCricketClubId, "Projector & Screen", EquipmentCategory.Electronics, EquipmentCondition.Excellent, "Long Room", 3000.00m, 20),
    };

    // ─────────────────────────────────────────────────────────────────────────
    // SESSIONS (6 months of history, 2-3 per week per club)
    // ─────────────────────────────────────────────────────────────────────────
    public static IReadOnlyList<SessionSeedDto> GenerateSessions()
    {
        var sessions = new List<SessionSeedDto>();
        var random = new Random(77);
        var startDate = DateTime.UtcNow.AddMonths(-6);
        var endDate = DateTime.UtcNow.AddMonths(1); // Include some future sessions
        int idx = 0;

        var clubSessionConfigs = new[]
        {
            new
            {
                ClubId = TeddingtonCricketClubId,
                Sessions = new[]
                {
                    (Title: "Senior Nets Practice", Category: SessionCategory.Seniors, Day: DayOfWeek.Tuesday, Hour: 18, Duration: 120, Capacity: 24, Fee: 5.00m),
                    (Title: "Junior Coaching", Category: SessionCategory.Juniors, Day: DayOfWeek.Wednesday, Hour: 16, Duration: 90, Capacity: 30, Fee: 3.00m),
                    (Title: "Weekend Match Prep", Category: SessionCategory.AllAges, Day: DayOfWeek.Thursday, Hour: 18, Duration: 90, Capacity: 20, Fee: 0m),
                }
            },
            new
            {
                ClubId = HighburyUnitedFCId,
                Sessions = new[]
                {
                    (Title: "First Team Training", Category: SessionCategory.Seniors, Day: DayOfWeek.Tuesday, Hour: 19, Duration: 90, Capacity: 25, Fee: 0m),
                    (Title: "Youth Academy", Category: SessionCategory.Juniors, Day: DayOfWeek.Saturday, Hour: 9, Duration: 120, Capacity: 40, Fee: 5.00m),
                    (Title: "5-a-side Social", Category: SessionCategory.Social, Day: DayOfWeek.Friday, Hour: 19, Duration: 60, Capacity: 20, Fee: 8.00m),
                }
            },
            new
            {
                ClubId = RichmondHockeyClubId,
                Sessions = new[]
                {
                    (Title: "Ladies Training", Category: SessionCategory.Ladies, Day: DayOfWeek.Monday, Hour: 19, Duration: 90, Capacity: 22, Fee: 0m),
                    (Title: "Men's Training", Category: SessionCategory.Mens, Day: DayOfWeek.Wednesday, Hour: 19, Duration: 90, Capacity: 22, Fee: 0m),
                    (Title: "Mixed Social Hockey", Category: SessionCategory.Mixed, Day: DayOfWeek.Sunday, Hour: 10, Duration: 90, Capacity: 30, Fee: 6.00m),
                }
            },
            new
            {
                ClubId = MaryleboneCricketClubId,
                Sessions = new[]
                {
                    (Title: "MCC Masterclass", Category: SessionCategory.Advanced, Day: DayOfWeek.Monday, Hour: 10, Duration: 120, Capacity: 12, Fee: 25.00m),
                    (Title: "Young Cricketers Programme", Category: SessionCategory.Juniors, Day: DayOfWeek.Saturday, Hour: 9, Duration: 180, Capacity: 30, Fee: 15.00m),
                    (Title: "Indoor Winter Nets", Category: SessionCategory.AllAges, Day: DayOfWeek.Thursday, Hour: 18, Duration: 90, Capacity: 16, Fee: 10.00m),
                }
            },
        };

        foreach (var clubConfig in clubSessionConfigs)
        {
            var currentDate = startDate;
            while (currentDate <= endDate)
            {
                foreach (var sessionDef in clubConfig.Sessions)
                {
                    if (currentDate.DayOfWeek == sessionDef.Day)
                    {
                        idx++;
                        var sessionStart = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day,
                            sessionDef.Hour, 0, 0, DateTimeKind.Utc);

                        var bookingCount = random.Next(
                            (int)(sessionDef.Capacity * 0.4),
                            Math.Min(sessionDef.Capacity, (int)(sessionDef.Capacity * 0.95)));

                        // Cancel ~5% of sessions randomly
                        var isCancelled = random.Next(100) < 5 && sessionStart < DateTime.UtcNow;

                        var sessionId = new Guid($"80000000-{idx:D4}-0000-0000-{idx:D12}");

                        sessions.Add(new SessionSeedDto(
                            sessionId, clubConfig.ClubId, sessionDef.Title, sessionDef.Category,
                            sessionStart, sessionDef.Duration, sessionDef.Capacity, sessionDef.Fee,
                            isCancelled ? 0 : bookingCount, isCancelled));
                    }
                }
                currentDate = currentDate.AddDays(1);
            }
        }

        return sessions;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // EVENTS (3-4 per club)
    // ─────────────────────────────────────────────────────────────────────────
    public static readonly IReadOnlyList<EventSeedDto> Events = new List<EventSeedDto>
    {
        // Teddington Cricket Club
        new(new("81000000-0000-0001-0001-000000000001"), TeddingtonCricketClubId, "Annual Awards Dinner", EventType.Social,
            DateTime.UtcNow.AddDays(30), DateTime.UtcNow.AddDays(30).AddHours(5), "Clubhouse", 80, true, 45.00m, 35.00m, EventStatus.RegistrationOpen),
        new(new("81000000-0000-0001-0002-000000000001"), TeddingtonCricketClubId, "Pre-Season AGM", EventType.AGM,
            DateTime.UtcNow.AddDays(14), DateTime.UtcNow.AddDays(14).AddHours(2), "Clubhouse", 100, false, null, null, EventStatus.Published),
        new(new("81000000-0000-0001-0003-000000000001"), TeddingtonCricketClubId, "Summer BBQ & Family Day", EventType.Social,
            DateTime.UtcNow.AddDays(60), DateTime.UtcNow.AddDays(60).AddHours(6), "Main Ground", 200, true, 15.00m, 10.00m, EventStatus.Draft),
        new(new("81000000-0000-0001-0004-000000000001"), TeddingtonCricketClubId, "Charity T20 Match", EventType.Fundraiser,
            DateTime.UtcNow.AddDays(45), DateTime.UtcNow.AddDays(45).AddHours(4), "Main Ground", 300, true, 10.00m, 5.00m, EventStatus.Published),

        // Highbury United FC
        new(new("81000000-0000-0002-0001-000000000001"), HighburyUnitedFCId, "End of Season Presentation", EventType.Presentation,
            DateTime.UtcNow.AddDays(21), DateTime.UtcNow.AddDays(21).AddHours(3), "Function Room", 120, true, 25.00m, 20.00m, EventStatus.RegistrationOpen),
        new(new("81000000-0000-0002-0002-000000000001"), HighburyUnitedFCId, "Youth Tournament", EventType.Tournament,
            DateTime.UtcNow.AddDays(35), DateTime.UtcNow.AddDays(35).AddHours(8), "Main Pitch", 200, false, null, null, EventStatus.Published),
        new(new("81000000-0000-0002-0003-000000000001"), HighburyUnitedFCId, "Quiz Night Fundraiser", EventType.Fundraiser,
            DateTime.UtcNow.AddDays(10), DateTime.UtcNow.AddDays(10).AddHours(3), "Function Room", 60, true, 12.00m, 10.00m, EventStatus.RegistrationOpen),

        // Richmond Hockey Club
        new(new("81000000-0000-0003-0001-000000000001"), RichmondHockeyClubId, "Mixed Tournament Day", EventType.Tournament,
            DateTime.UtcNow.AddDays(28), DateTime.UtcNow.AddDays(28).AddHours(7), "Astroturf Pitch 1", 150, true, 20.00m, 15.00m, EventStatus.RegistrationOpen),
        new(new("81000000-0000-0003-0002-000000000001"), RichmondHockeyClubId, "Annual General Meeting", EventType.AGM,
            DateTime.UtcNow.AddDays(7), DateTime.UtcNow.AddDays(7).AddHours(2), "Meeting Room", 50, false, null, null, EventStatus.Published),
        new(new("81000000-0000-0003-0003-000000000001"), RichmondHockeyClubId, "Sponsors Evening", EventType.Social,
            DateTime.UtcNow.AddDays(42), DateTime.UtcNow.AddDays(42).AddHours(4), "Clubhouse & Bar", 80, true, 35.00m, 25.00m, EventStatus.Draft),

        // Marylebone Cricket Club
        new(new("81000000-0000-0004-0001-000000000001"), MaryleboneCricketClubId, "Spirit of Cricket Lecture", EventType.Presentation,
            DateTime.UtcNow.AddDays(18), DateTime.UtcNow.AddDays(18).AddHours(2), "Long Room", 150, true, 30.00m, 20.00m, EventStatus.RegistrationOpen),
        new(new("81000000-0000-0004-0002-000000000001"), MaryleboneCricketClubId, "Members' Day", EventType.Social,
            DateTime.UtcNow.AddDays(50), DateTime.UtcNow.AddDays(50).AddHours(8), "Lord's Main Ground", 500, false, null, null, EventStatus.Published),
        new(new("81000000-0000-0004-0003-000000000001"), MaryleboneCricketClubId, "Coaching Masterclass", EventType.Training,
            DateTime.UtcNow.AddDays(25), DateTime.UtcNow.AddDays(25).AddHours(4), "Indoor School", 24, true, 75.00m, 50.00m, EventStatus.RegistrationOpen),
        new(new("81000000-0000-0004-0004-000000000001"), MaryleboneCricketClubId, "Heritage Tour & Dinner", EventType.Social,
            DateTime.UtcNow.AddDays(55), DateTime.UtcNow.AddDays(55).AddHours(5), "Lord's Main Ground", 40, true, 95.00m, 75.00m, EventStatus.Draft),
    };

    // ─────────────────────────────────────────────────────────────────────────
    // SEASONS & COMPETITIONS (1 active competition per club with teams)
    // ─────────────────────────────────────────────────────────────────────────
    public static readonly IReadOnlyList<SeasonSeedDto> Seasons = new List<SeasonSeedDto>
    {
        new(new("82000000-0000-0001-0001-000000000001"), TeddingtonCricketClubId, "2024 Season", new DateTime(2024, 4, 1), new DateTime(2024, 9, 30), true),
        new(new("82000000-0000-0002-0001-000000000001"), HighburyUnitedFCId, "2024/25 Season", new DateTime(2024, 8, 1), new DateTime(2025, 5, 31), true),
        new(new("82000000-0000-0003-0001-000000000001"), RichmondHockeyClubId, "2024/25 Season", new DateTime(2024, 9, 1), new DateTime(2025, 4, 30), true),
        new(new("82000000-0000-0004-0001-000000000001"), MaryleboneCricketClubId, "2024 Season", new DateTime(2024, 4, 1), new DateTime(2024, 9, 30), true),
    };

    public static readonly IReadOnlyList<CompetitionSeedDto> Competitions = new List<CompetitionSeedDto>
    {
        new(new("83000000-0000-0001-0001-000000000001"), TeddingtonCricketClubId, new("82000000-0000-0001-0001-000000000001"),
            "Saturday League Division 2", CompetitionType.League, "Active", 12, 4, 0),
        new(new("83000000-0000-0002-0001-000000000001"), HighburyUnitedFCId, new("82000000-0000-0002-0001-000000000001"),
            "Middlesex Sunday League", CompetitionType.League, "Active", 3, 1, 0),
        new(new("83000000-0000-0003-0001-000000000001"), RichmondHockeyClubId, new("82000000-0000-0003-0001-000000000001"),
            "Surrey Hockey League Div 1", CompetitionType.League, "Active", 3, 1, 0),
        new(new("83000000-0000-0004-0001-000000000001"), MaryleboneCricketClubId, new("82000000-0000-0004-0001-000000000001"),
            "Inter-Club Championship", CompetitionType.Championship, "Active", 20, 6, 0),
    };

    public static readonly IReadOnlyList<CompetitionTeamSeedDto> CompetitionTeams = new List<CompetitionTeamSeedDto>
    {
        // Teddington Cricket Club - Saturday League
        new(new("84000000-0000-0001-0001-000000000001"), TeddingtonCricketClubId, new("83000000-0000-0001-0001-000000000001"), "Teddington 1st XI", "#1B5E20", 15),
        new(new("84000000-0000-0001-0002-000000000001"), TeddingtonCricketClubId, new("83000000-0000-0001-0001-000000000001"), "Teddington 2nd XI", "#4CAF50", 15),
        new(new("84000000-0000-0001-0003-000000000001"), TeddingtonCricketClubId, new("83000000-0000-0001-0001-000000000001"), "Hampton Hill CC", "#FF6F00", 11),
        new(new("84000000-0000-0001-0004-000000000001"), TeddingtonCricketClubId, new("83000000-0000-0001-0001-000000000001"), "Sunbury CC", "#1565C0", 11),

        // Highbury United FC - Sunday League
        new(new("84000000-0000-0002-0001-000000000001"), HighburyUnitedFCId, new("83000000-0000-0002-0001-000000000001"), "Highbury United 1st", "#B71C1C", 18),
        new(new("84000000-0000-0002-0002-000000000001"), HighburyUnitedFCId, new("83000000-0000-0002-0001-000000000001"), "Highbury United Reserves", "#F44336", 16),
        new(new("84000000-0000-0002-0003-000000000001"), HighburyUnitedFCId, new("83000000-0000-0002-0001-000000000001"), "Arsenal Community FC", "#EF6C00", 16),
        new(new("84000000-0000-0002-0004-000000000001"), HighburyUnitedFCId, new("83000000-0000-0002-0001-000000000001"), "Islington Rangers", "#2E7D32", 16),

        // Richmond Hockey Club - Surrey League
        new(new("84000000-0000-0003-0001-000000000001"), RichmondHockeyClubId, new("83000000-0000-0003-0001-000000000001"), "Richmond Men's 1st", "#0D47A1", 16),
        new(new("84000000-0000-0003-0002-000000000001"), RichmondHockeyClubId, new("83000000-0000-0003-0001-000000000001"), "Richmond Ladies 1st", "#AD1457", 16),
        new(new("84000000-0000-0003-0003-000000000001"), RichmondHockeyClubId, new("83000000-0000-0003-0001-000000000001"), "Surbiton HC", "#FF6F00", 16),
        new(new("84000000-0000-0003-0004-000000000001"), RichmondHockeyClubId, new("83000000-0000-0003-0001-000000000001"), "Wimbledon HC", "#4A148C", 16),

        // Marylebone Cricket Club - Championship
        new(new("84000000-0000-0004-0001-000000000001"), MaryleboneCricketClubId, new("83000000-0000-0004-0001-000000000001"), "MCC President's XI", "#E65100", 15),
        new(new("84000000-0000-0004-0002-000000000001"), MaryleboneCricketClubId, new("83000000-0000-0004-0001-000000000001"), "MCC Secretary's XI", "#1A237E", 15),
        new(new("84000000-0000-0004-0003-000000000001"), MaryleboneCricketClubId, new("83000000-0000-0004-0001-000000000001"), "Cross Arrows CC", "#004D40", 11),
        new(new("84000000-0000-0004-0004-000000000001"), MaryleboneCricketClubId, new("83000000-0000-0004-0001-000000000001"), "I Zingari", "#880E4F", 11),
    };

    // ─────────────────────────────────────────────────────────────────────────
    // PAYMENTS (12 months of payment history)
    // ─────────────────────────────────────────────────────────────────────────
    public static IReadOnlyList<PaymentSeedDto> GeneratePayments()
    {
        var payments = new List<PaymentSeedDto>();
        var random = new Random(123);
        var startDate = DateTime.UtcNow.AddMonths(-12);
        int idx = 0;

        foreach (var member in Members)
        {
            if (member.Status == MemberStatus.Pending) continue;

            // Membership payment
            idx++;
            var clubTypes = MembershipTypes.Where(mt => mt.ClubId == member.ClubId).ToList();
            var membershipType = clubTypes[idx % clubTypes.Count];
            var membershipPaymentDate = member.JoinedDate.AddDays(random.Next(0, 7));

            payments.Add(new PaymentSeedDto(
                new Guid($"90000000-{idx:D4}-0000-0000-{idx:D12}"),
                member.ClubId, member.Id, membershipType.Price, PaymentMethod.Stripe,
                PaymentType.Membership, PaymentStatus.Completed, membershipPaymentDate,
                $"Membership: {membershipType.Name}"));

            // Session fees (2-5 session payments per active member over 12 months)
            if (member.Status == MemberStatus.Active)
            {
                var sessionPaymentCount = random.Next(2, 6);
                for (int s = 0; s < sessionPaymentCount; s++)
                {
                    idx++;
                    var sessionFee = new[] { 5.00m, 6.00m, 8.00m, 10.00m, 15.00m, 25.00m }[random.Next(6)];
                    var payDate = startDate.AddDays(random.Next(0, 365));
                    var method = random.Next(10) < 7 ? PaymentMethod.Stripe : PaymentMethod.Cash;
                    var status = random.Next(100) < 95 ? PaymentStatus.Completed : PaymentStatus.Failed;

                    payments.Add(new PaymentSeedDto(
                        new Guid($"90000000-{idx:D4}-0000-0000-{idx:D12}"),
                        member.ClubId, member.Id, sessionFee, method,
                        PaymentType.SessionFee, status, payDate,
                        "Session attendance fee"));
                }

                // Event ticket payments (0-2 per member)
                var eventPaymentCount = random.Next(0, 3);
                for (int e = 0; e < eventPaymentCount; e++)
                {
                    idx++;
                    var ticketPrice = new[] { 10.00m, 15.00m, 20.00m, 25.00m, 35.00m, 45.00m, 75.00m }[random.Next(7)];
                    var payDate = startDate.AddDays(random.Next(0, 365));

                    payments.Add(new PaymentSeedDto(
                        new Guid($"90000000-{idx:D4}-0000-0000-{idx:D12}"),
                        member.ClubId, member.Id, ticketPrice, PaymentMethod.Stripe,
                        PaymentType.EventTicket, PaymentStatus.Completed, payDate,
                        "Event ticket purchase"));
                }
            }
        }

        return payments;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SUBSCRIPTIONS (1 per club)
    // ─────────────────────────────────────────────────────────────────────────
    public static readonly IReadOnlyList<ClubSubscriptionSeedDto> Subscriptions = new List<ClubSubscriptionSeedDto>
    {
        new(new("A0000000-0000-0001-0001-000000000001"), TeddingtonCricketClubId, SubscriptionTier.Pro, true,
            new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddDays(-1),
            99.00m, BillingCycle.Monthly),
        new(new("A0000000-0000-0002-0001-000000000001"), HighburyUnitedFCId, SubscriptionTier.Starter, true,
            new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddDays(-1),
            49.00m, BillingCycle.Monthly),
        new(new("A0000000-0000-0003-0001-000000000001"), RichmondHockeyClubId, SubscriptionTier.Pro, true,
            new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddDays(-1),
            99.00m, BillingCycle.Monthly),
        new(new("A0000000-0000-0004-0001-000000000001"), MaryleboneCricketClubId, SubscriptionTier.Enterprise, true,
            new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1).AddDays(-1),
            249.00m, BillingCycle.Monthly),
    };

    // ─────────────────────────────────────────────────────────────────────────
    // SESSION BOOKINGS (5-10 per club for recent/future sessions)
    // ─────────────────────────────────────────────────────────────────────────
    public static IReadOnlyList<SessionBookingSeedDto> GenerateSessionBookings()
    {
        var bookings = new List<SessionBookingSeedDto>();
        var sessions = GenerateSessions();
        var random = new Random(200);
        int idx = 0;

        // Get future and recent sessions (last 2 weeks + next month)
        var cutoffPast = DateTime.UtcNow.AddDays(-14);
        var cutoffFuture = DateTime.UtcNow.AddMonths(1);
        var recentSessions = sessions
            .Where(s => !s.IsCancelled && s.StartTime >= cutoffPast && s.StartTime <= cutoffFuture)
            .ToList();

        var clubIds = new[] { TeddingtonCricketClubId, HighburyUnitedFCId, RichmondHockeyClubId, MaryleboneCricketClubId };

        foreach (var clubId in clubIds)
        {
            var clubSessions = recentSessions.Where(s => s.ClubId == clubId).Take(10).ToList();
            var activeMembers = Members.Where(m => m.ClubId == clubId && m.Status == MemberStatus.Active).ToList();

            foreach (var session in clubSessions)
            {
                // Book 5-8 members per session
                var bookingCount = Math.Min(random.Next(5, 9), activeMembers.Count);
                var shuffled = activeMembers.OrderBy(_ => random.Next()).Take(bookingCount).ToList();

                foreach (var member in shuffled)
                {
                    idx++;
                    var status = session.StartTime < DateTime.UtcNow
                        ? (random.Next(10) < 8 ? BookingStatus.Attended : BookingStatus.NoShow)
                        : BookingStatus.Confirmed;

                    var bookingId = new Guid($"A1000000-{idx:D4}-0000-0000-{idx:D12}");
                    var bookedAt = session.StartTime.AddDays(-random.Next(1, 7));

                    bookings.Add(new SessionBookingSeedDto(
                        bookingId, session.Id, member.Id, clubId,
                        status, bookedAt, session.Fee));
                }
            }
        }

        return bookings;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // EVENT REGISTRATIONS (3-8 per event with RegistrationOpen status)
    // ─────────────────────────────────────────────────────────────────────────
    public static IReadOnlyList<EventRegistrationSeedDto> GenerateEventRegistrations()
    {
        var registrations = new List<EventRegistrationSeedDto>();
        var random = new Random(300);
        int idx = 0;

        var openEvents = Events.Where(e => e.Status == EventStatus.RegistrationOpen).ToList();

        foreach (var evt in openEvents)
        {
            var activeMembers = Members.Where(m => m.ClubId == evt.ClubId && m.Status == MemberStatus.Active).ToList();
            var regCount = Math.Min(random.Next(3, 9), activeMembers.Count);
            var registrants = activeMembers.OrderBy(_ => random.Next()).Take(regCount).ToList();

            foreach (var member in registrants)
            {
                idx++;
                var regId = new Guid($"A2000000-{idx:D4}-0000-0000-{idx:D12}");
                var registeredAt = DateTime.UtcNow.AddDays(-random.Next(1, 14));
                string? ticketNumber = null;
                string? qrCode = null;

                if (evt.IsTicketed)
                {
                    ticketNumber = $"TKT-{evt.ClubId.ToString()[..4].ToUpper()}-{idx:D6}";
                    qrCode = $"QR-{regId.ToString()[..8].ToUpper()}-{ticketNumber}";
                }

                var pricePaid = evt.MemberPrice ?? evt.StandardPrice ?? 0m;

                registrations.Add(new EventRegistrationSeedDto(
                    regId, evt.Id, member.Id, evt.ClubId,
                    registeredAt, ticketNumber, qrCode, pricePaid, true));
            }
        }

        return registrations;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // COMPETITION MATCHES & STANDINGS (6 matches per competition)
    // ─────────────────────────────────────────────────────────────────────────
    public static IReadOnlyList<MatchSeedDto> GenerateMatches()
    {
        var matches = new List<MatchSeedDto>();
        var random = new Random(400);
        int idx = 0;

        foreach (var competition in Competitions)
        {
            var teams = CompetitionTeams.Where(t => t.CompetitionId == competition.Id).ToList();
            if (teams.Count < 2) continue;

            // Generate 6 matches: 3 completed, 2 scheduled, 1 in progress
            var matchDate = DateTime.UtcNow.AddDays(-42); // Start 6 weeks ago

            for (int m = 0; m < 6; m++)
            {
                idx++;
                var homeTeam = teams[m % teams.Count];
                var awayTeam = teams[(m + 1) % teams.Count];
                var matchId = new Guid($"A3000000-{idx:D4}-0000-0000-{idx:D12}");

                MatchStatus status;
                MatchResult result;
                int? homeScore = null;
                int? awayScore = null;

                if (m < 3) // Completed
                {
                    status = MatchStatus.Completed;
                    homeScore = random.Next(0, 5);
                    awayScore = random.Next(0, 5);
                    result = homeScore > awayScore ? MatchResult.HomeWin
                           : awayScore > homeScore ? MatchResult.AwayWin
                           : MatchResult.Draw;
                }
                else if (m == 3) // In progress
                {
                    status = MatchStatus.InProgress;
                    result = MatchResult.NotPlayed;
                    homeScore = random.Next(0, 3);
                    awayScore = random.Next(0, 3);
                    matchDate = DateTime.UtcNow.AddHours(-1);
                }
                else // Scheduled
                {
                    status = MatchStatus.Scheduled;
                    result = MatchResult.NotPlayed;
                    matchDate = DateTime.UtcNow.AddDays(7 * (m - 3));
                }

                var venue = competition.ClubId == TeddingtonCricketClubId ? "Main Cricket Ground"
                          : competition.ClubId == HighburyUnitedFCId ? "Main Pitch"
                          : competition.ClubId == RichmondHockeyClubId ? "Astroturf Pitch 1"
                          : "Lord's Main Ground";

                matches.Add(new MatchSeedDto(
                    matchId, competition.Id, competition.ClubId,
                    homeTeam.Id, awayTeam.Id,
                    matchDate, venue, status, result,
                    homeScore, awayScore, m + 1));

                if (m < 3) matchDate = matchDate.AddDays(14);
            }
        }

        return matches;
    }

    public static IReadOnlyList<StandingSeedDto> GenerateStandings()
    {
        var standings = new List<StandingSeedDto>();
        var matches = GenerateMatches();
        int idx = 0;

        foreach (var competition in Competitions)
        {
            var teams = CompetitionTeams.Where(t => t.CompetitionId == competition.Id).ToList();
            var compMatches = matches.Where(m => m.CompetitionId == competition.Id && m.Status == MatchStatus.Completed).ToList();

            foreach (var team in teams)
            {
                idx++;
                int played = 0, won = 0, drawn = 0, lost = 0, goalsFor = 0, goalsAgainst = 0;

                foreach (var match in compMatches)
                {
                    if (match.HomeTeamId == team.Id)
                    {
                        played++;
                        goalsFor += match.HomeScore ?? 0;
                        goalsAgainst += match.AwayScore ?? 0;
                        if (match.Result == MatchResult.HomeWin) won++;
                        else if (match.Result == MatchResult.Draw) drawn++;
                        else lost++;
                    }
                    else if (match.AwayTeamId == team.Id)
                    {
                        played++;
                        goalsFor += match.AwayScore ?? 0;
                        goalsAgainst += match.HomeScore ?? 0;
                        if (match.Result == MatchResult.AwayWin) won++;
                        else if (match.Result == MatchResult.Draw) drawn++;
                        else lost++;
                    }
                }

                int points = (won * competition.PointsForWin) + (drawn * competition.PointsForDraw) + (lost * competition.PointsForLoss);
                var standingId = new Guid($"A4000000-{idx:D4}-0000-0000-{idx:D12}");

                standings.Add(new StandingSeedDto(
                    standingId, competition.Id, team.Id, competition.ClubId,
                    played, won, drawn, lost, goalsFor, goalsAgainst, points));
            }
        }

        // Sort by points descending and assign positions
        var result = new List<StandingSeedDto>();
        foreach (var competition in Competitions)
        {
            var compStandings = standings
                .Where(s => s.CompetitionId == competition.Id)
                .OrderByDescending(s => s.Points)
                .ThenByDescending(s => s.GoalsFor - s.GoalsAgainst)
                .ToList();

            for (int i = 0; i < compStandings.Count; i++)
            {
                result.Add(compStandings[i] with { Position = i + 1 });
            }
        }

        return result;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // INVOICES (2-3 outstanding per club)
    // ─────────────────────────────────────────────────────────────────────────
    public static IReadOnlyList<InvoiceSeedDto> GenerateInvoices()
    {
        var invoices = new List<InvoiceSeedDto>();
        var random = new Random(500);
        int idx = 0;

        var clubIds = new[] { TeddingtonCricketClubId, HighburyUnitedFCId, RichmondHockeyClubId, MaryleboneCricketClubId };

        foreach (var clubId in clubIds)
        {
            var activeMembers = Members.Where(m => m.ClubId == clubId && m.Status == MemberStatus.Active).ToList();
            var invoiceCount = random.Next(2, 4); // 2-3 per club

            for (int i = 0; i < invoiceCount && i < activeMembers.Count; i++)
            {
                idx++;
                var member = activeMembers[i];
                var invoiceId = new Guid($"A5000000-{idx:D4}-0000-0000-{idx:D12}");
                var invoiceNumber = $"INV-{clubId.ToString()[..4].ToUpper()}-{idx:D5}";
                var issuedDate = DateTime.UtcNow.AddDays(-random.Next(14, 45));
                var dueDate = issuedDate.AddDays(30);
                var status = dueDate < DateTime.UtcNow ? InvoiceStatus.Overdue : InvoiceStatus.Sent;

                var clubTypes = MembershipTypes.Where(mt => mt.ClubId == clubId).ToList();
                var membershipType = clubTypes[idx % clubTypes.Count];
                var amount = membershipType.Price;

                invoices.Add(new InvoiceSeedDto(
                    invoiceId, clubId, member.Id, invoiceNumber,
                    amount, status, issuedDate, dueDate,
                    $"Membership renewal: {membershipType.Name}"));
            }
        }

        return invoices;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // MEMBER BALANCES (for all active members)
    // ─────────────────────────────────────────────────────────────────────────
    public static IReadOnlyList<MemberBalanceSeedDto> GenerateMemberBalances()
    {
        var balances = new List<MemberBalanceSeedDto>();
        var random = new Random(600);
        int idx = 0;

        foreach (var member in Members)
        {
            if (member.Status != MemberStatus.Active) continue;

            idx++;
            var balanceId = new Guid($"A6000000-{idx:D4}-0000-0000-{idx:D12}");

            // ~40% of members have an outstanding balance
            decimal outstanding = 0m;
            decimal credit = 0m;

            if (idx % 5 < 2) // 40% have outstanding
            {
                outstanding = new[] { 25.00m, 50.00m, 75.00m, 95.00m, 120.00m, 180.00m, 285.00m }[random.Next(7)];
            }
            else if (idx % 10 == 0) // 10% have credit
            {
                credit = new[] { 10.00m, 15.00m, 25.00m, 50.00m }[random.Next(4)];
            }

            var lastPaymentDate = DateTime.UtcNow.AddDays(-random.Next(1, 90));

            balances.Add(new MemberBalanceSeedDto(
                balanceId, member.Id, member.ClubId,
                outstanding, credit, lastPaymentDate));
        }

        return balances;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // COMMUNICATION TEMPLATES (6 per club)
    // ─────────────────────────────────────────────────────────────────────────
    public static IReadOnlyList<CommunicationTemplateSeedDto> GenerateCommunicationTemplates()
    {
        var templates = new List<CommunicationTemplateSeedDto>();
        int idx = 0;

        var clubIds = new[] { TeddingtonCricketClubId, HighburyUnitedFCId, RichmondHockeyClubId, MaryleboneCricketClubId };

        var templateDefs = new[]
        {
            (Type: "Welcome", Subject: "Welcome to {{ClubName}}!", Body: "Dear {{FirstName}},\n\nWelcome to {{ClubName}}! We're thrilled to have you as a member.\n\nYour membership number is {{MemberNumber}}.\n\nBest regards,\n{{ClubName}} Team"),
            (Type: "PasswordReset", Subject: "Reset Your Password - {{ClubName}}", Body: "Hi {{FirstName}},\n\nWe received a request to reset your password. Click the link below to set a new password:\n\n{{ResetLink}}\n\nIf you didn't request this, please ignore this email.\n\nThanks,\n{{ClubName}}"),
            (Type: "PaymentReminder", Subject: "Payment Reminder - {{Amount}} Due", Body: "Dear {{FirstName}},\n\nThis is a friendly reminder that your payment of £{{Amount}} is due on {{DueDate}}.\n\nPlease log in to your account to make a payment.\n\nRegards,\n{{ClubName}} Treasurer"),
            (Type: "BookingConfirmation", Subject: "Booking Confirmed - {{SessionTitle}}", Body: "Hi {{FirstName}},\n\nYour booking for {{SessionTitle}} on {{SessionDate}} at {{SessionTime}} has been confirmed.\n\nSee you there!\n{{ClubName}}"),
            (Type: "EventNotification", Subject: "Upcoming Event: {{EventTitle}}", Body: "Dear {{FirstName}},\n\n{{EventTitle}} is coming up on {{EventDate}}.\n\nVenue: {{Venue}}\nTime: {{StartTime}}\n\n{{#if IsTicketed}}Tickets are available at £{{Price}}.{{/if}}\n\nWe hope to see you there!\n{{ClubName}}"),
            (Type: "MembershipRenewal", Subject: "Your Membership is Due for Renewal", Body: "Dear {{FirstName}},\n\nYour {{MembershipType}} membership at {{ClubName}} expires on {{ExpiryDate}}.\n\nRenewal fee: £{{RenewalAmount}}\n\nPlease renew online or contact us.\n\nBest regards,\n{{ClubName}} Membership Team"),
        };

        foreach (var clubId in clubIds)
        {
            foreach (var def in templateDefs)
            {
                idx++;
                var templateId = new Guid($"A7000000-{idx:D4}-0000-0000-{idx:D12}");

                templates.Add(new CommunicationTemplateSeedDto(
                    templateId, clubId, def.Type, def.Subject, def.Body, true));
            }
        }

        return templates;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // FAMILY MEMBERS (2-3 family accounts per club)
    // ─────────────────────────────────────────────────────────────────────────
    public static IReadOnlyList<FamilyGroupSeedDto> GenerateFamilyGroups()
    {
        var groups = new List<FamilyGroupSeedDto>();
        int idx = 0;

        var clubIds = new[] { TeddingtonCricketClubId, HighburyUnitedFCId, RichmondHockeyClubId, MaryleboneCricketClubId };

        foreach (var clubId in clubIds)
        {
            var activeMembers = Members.Where(m => m.ClubId == clubId && m.Status == MemberStatus.Active).ToList();
            if (activeMembers.Count < 6) continue;

            // Create 2-3 family groups per club
            var familyCount = Math.Min(3, activeMembers.Count / 3);

            for (int f = 0; f < familyCount; f++)
            {
                idx++;
                var groupId = new Guid($"A8000000-{idx:D4}-0000-0000-{idx:D12}");
                var primaryMember = activeMembers[f * 3];
                var familyName = primaryMember.LastName;

                groups.Add(new FamilyGroupSeedDto(
                    groupId, clubId, primaryMember.Id, $"The {familyName} Family"));
            }
        }

        return groups;
    }

    public static IReadOnlyList<FamilyMemberLinkSeedDto> GenerateFamilyMemberLinks()
    {
        var links = new List<FamilyMemberLinkSeedDto>();
        var groups = GenerateFamilyGroups();
        int idx = 0;

        foreach (var group in groups)
        {
            var activeMembers = Members.Where(m => m.ClubId == group.ClubId && m.Status == MemberStatus.Active).ToList();
            var primaryIdx = activeMembers.FindIndex(m => m.Id == group.PrimaryMemberId);
            if (primaryIdx < 0 || primaryIdx + 2 >= activeMembers.Count) continue;

            // Primary member is the head
            idx++;
            links.Add(new FamilyMemberLinkSeedDto(
                new Guid($"A9000000-{idx:D4}-0000-0000-{idx:D12}"),
                group.Id, group.PrimaryMemberId, FamilyMemberRelation.Parent));

            // Spouse
            idx++;
            var spouse = activeMembers[primaryIdx + 1];
            links.Add(new FamilyMemberLinkSeedDto(
                new Guid($"A9000000-{idx:D4}-0000-0000-{idx:D12}"),
                group.Id, spouse.Id, FamilyMemberRelation.Spouse));

            // Child
            idx++;
            var child = activeMembers[primaryIdx + 2];
            links.Add(new FamilyMemberLinkSeedDto(
                new Guid($"A9000000-{idx:D4}-0000-0000-{idx:D12}"),
                group.Id, child.Id, FamilyMemberRelation.Child));
        }

        return links;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // SUMMARY STATISTICS (for quick reference)
    // ─────────────────────────────────────────────────────────────────────────
    public static SeedSummary GetSummary()
    {
        var sessions = GenerateSessions();
        var payments = GeneratePayments();
        var memberships = GenerateMemberships();
        var bookings = GenerateSessionBookings();
        var registrations = GenerateEventRegistrations();
        var matches = GenerateMatches();
        var standings = GenerateStandings();
        var invoices = GenerateInvoices();
        var balances = GenerateMemberBalances();
        var templates = GenerateCommunicationTemplates();
        var familyGroups = GenerateFamilyGroups();
        var familyLinks = GenerateFamilyMemberLinks();

        return new SeedSummary(
            ClubCount: Clubs.Count,
            ManagerCount: ClubManagers.Count,
            MemberCount: Members.Count,
            MembershipTypeCount: MembershipTypes.Count,
            MembershipCount: memberships.Count,
            SessionCount: sessions.Count,
            EventCount: Events.Count,
            CompetitionCount: Competitions.Count,
            TeamCount: CompetitionTeams.Count,
            PaymentCount: payments.Count,
            FacilityCount: Facilities.Count,
            EquipmentCount: Equipment.Count,
            SubscriptionCount: Subscriptions.Count,
            SessionBookingCount: bookings.Count,
            EventRegistrationCount: registrations.Count,
            MatchCount: matches.Count,
            StandingCount: standings.Count,
            InvoiceCount: invoices.Count,
            MemberBalanceCount: balances.Count,
            CommunicationTemplateCount: templates.Count,
            FamilyGroupCount: familyGroups.Count,
            FamilyMemberLinkCount: familyLinks.Count
        );
    }
}


// ─────────────────────────────────────────────────────────────────────────────
// SEED DATA DTOs
// ─────────────────────────────────────────────────────────────────────────────

public record ClubSeedDto(
    Guid Id, string Name, string Slug, ClubType ClubType,
    string PrimaryColor, string SecondaryColor, string? AccentColor,
    string ContactEmail, string ContactPhone, string Address, string Website);

public record UserSeedDto(
    Guid Id, string Email, string FirstName, string LastName,
    Guid? ClubId, string Role);

public record MemberSeedDto(
    Guid Id, Guid ClubId, string MemberNumber, string FirstName, string LastName,
    string Email, string Phone, DateTime DateOfBirth, Gender Gender,
    MemberStatus Status, DateTime JoinedDate);

public record MembershipTypeSeedDto(
    Guid Id, Guid ClubId, string Name, string? Description,
    decimal Price, BillingCycle BillingCycle,
    int? MinAge, int? MaxAge, int? Capacity, decimal JoiningFee);

public record MembershipSeedDto(
    Guid Id, Guid ClubId, Guid MemberId, Guid MembershipTypeId,
    DateTime StartDate, DateTime EndDate, MembershipStatus Status,
    decimal PricePaid, bool AutoRenew);

public record FacilitySeedDto(
    Guid Id, Guid ClubId, string Name, string FacilityType,
    string Description, int Capacity);

public record EquipmentSeedDto(
    Guid Id, Guid ClubId, string Name, EquipmentCategory Category,
    EquipmentCondition Condition, string Location, decimal Value,
    decimal AnnualDepreciationRate);

public record SessionSeedDto(
    Guid Id, Guid ClubId, string Title, SessionCategory Category,
    DateTime StartTime, int Duration, int Capacity, decimal Fee,
    int CurrentBookingCount, bool IsCancelled);

public record EventSeedDto(
    Guid Id, Guid ClubId, string Title, EventType EventType,
    DateTime StartDateTime, DateTime EndDateTime, string VenueName,
    int Capacity, bool IsTicketed, decimal? StandardPrice, decimal? MemberPrice,
    EventStatus Status);

public record SeasonSeedDto(
    Guid Id, Guid ClubId, string Name, DateTime StartDate, DateTime EndDate, bool IsActive);

public record CompetitionSeedDto(
    Guid Id, Guid ClubId, Guid SeasonId, string Name,
    CompetitionType CompetitionType, string Status,
    int PointsForWin, int PointsForDraw, int PointsForLoss);

public record CompetitionTeamSeedDto(
    Guid Id, Guid ClubId, Guid CompetitionId, string TeamName,
    string TeamColor, int SquadSize);

public record PaymentSeedDto(
    Guid Id, Guid ClubId, Guid MemberId, decimal Amount,
    PaymentMethod Method, PaymentType Type, PaymentStatus Status,
    DateTime PaymentDate, string? Description);

public record SeedSummary(
    int ClubCount, int ManagerCount, int MemberCount,
    int MembershipTypeCount, int MembershipCount,
    int SessionCount, int EventCount, int CompetitionCount,
    int TeamCount, int PaymentCount, int FacilityCount, int EquipmentCount,
    int SubscriptionCount, int SessionBookingCount, int EventRegistrationCount,
    int MatchCount, int StandingCount, int InvoiceCount,
    int MemberBalanceCount, int CommunicationTemplateCount,
    int FamilyGroupCount, int FamilyMemberLinkCount);

// ─────────────────────────────────────────────────────────────────────────────
// NEW SEED DATA DTOs
// ─────────────────────────────────────────────────────────────────────────────

public record ClubSubscriptionSeedDto(
    Guid Id, Guid ClubId, SubscriptionTier Tier, bool IsActive,
    DateTime BillingPeriodStart, DateTime BillingPeriodEnd,
    decimal MonthlyPrice, BillingCycle BillingCycle);

public record SessionBookingSeedDto(
    Guid Id, Guid SessionId, Guid MemberId, Guid ClubId,
    BookingStatus Status, DateTime BookedAt, decimal FeePaid);

public record EventRegistrationSeedDto(
    Guid Id, Guid EventId, Guid MemberId, Guid ClubId,
    DateTime RegisteredAt, string? TicketNumber, string? QrCode,
    decimal PricePaid, bool IsConfirmed);

public record MatchSeedDto(
    Guid Id, Guid CompetitionId, Guid ClubId,
    Guid HomeTeamId, Guid AwayTeamId,
    DateTime MatchDate, string Venue, MatchStatus Status, MatchResult Result,
    int? HomeScore, int? AwayScore, int MatchDay);

public record StandingSeedDto(
    Guid Id, Guid CompetitionId, Guid TeamId, Guid ClubId,
    int Played, int Won, int Drawn, int Lost,
    int GoalsFor, int GoalsAgainst, int Points, int Position = 0);

public record InvoiceSeedDto(
    Guid Id, Guid ClubId, Guid MemberId, string InvoiceNumber,
    decimal Amount, InvoiceStatus Status, DateTime IssuedDate, DateTime DueDate,
    string? Description);

public record MemberBalanceSeedDto(
    Guid Id, Guid MemberId, Guid ClubId,
    decimal OutstandingAmount, decimal CreditAmount, DateTime LastPaymentDate);

public record CommunicationTemplateSeedDto(
    Guid Id, Guid ClubId, string TemplateType, string Subject,
    string Body, bool IsActive);

public record FamilyGroupSeedDto(
    Guid Id, Guid ClubId, Guid PrimaryMemberId, string FamilyName);

public record FamilyMemberLinkSeedDto(
    Guid Id, Guid FamilyGroupId, Guid MemberId, FamilyMemberRelation Relation);
