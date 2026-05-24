using TheLeague.Shared.Domain.Entities;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Clubs.Domain;

public class SportConfiguration : BaseEntity
{
    public Guid ClubId { get; private set; }
    public ClubType SportType { get; private set; }
    public string? DefaultSessionCategories { get; private set; } // JSON array
    public string? DefaultCompetitionTypes { get; private set; } // JSON array
    public string? DefaultMatchEventTypes { get; private set; } // JSON array
    public string? ScoreFields { get; private set; } // JSON: {"fields": ["runs", "wickets", "overs"]}

    public static SportConfiguration CreateForSport(Guid clubId, ClubType sportType)
    {
        return new SportConfiguration
        {
            ClubId = clubId,
            SportType = sportType,
            DefaultSessionCategories = GetDefaultCategories(sportType),
            DefaultCompetitionTypes = GetDefaultCompetitionTypes(sportType),
            DefaultMatchEventTypes = GetDefaultMatchEvents(sportType),
            ScoreFields = GetScoreFields(sportType)
        };
    }

    private static string GetDefaultCategories(ClubType sport) => sport switch
    {
        ClubType.Cricket => "[\"Seniors\",\"Juniors\",\"Ladies\",\"AllAges\",\"Nets\"]",
        ClubType.Football => "[\"Seniors\",\"Juniors\",\"Ladies\",\"Veterans\",\"Training\"]",
        ClubType.Hockey => "[\"Seniors\",\"Juniors\",\"Mixed\",\"Training\"]",
        ClubType.Tennis => "[\"Singles\",\"Doubles\",\"Mixed\",\"Coaching\",\"Social\"]",
        _ => "[\"AllAges\",\"Juniors\",\"Seniors\",\"Training\",\"Social\"]"
    };

    private static string GetDefaultCompetitionTypes(ClubType sport) => sport switch
    {
        ClubType.Cricket => "[\"League\",\"Cup\",\"Friendly\",\"Tournament\"]",
        ClubType.Football => "[\"League\",\"Cup\",\"Friendly\",\"Tournament\",\"Knockout\"]",
        _ => "[\"League\",\"Tournament\",\"Cup\",\"Friendly\"]"
    };

    private static string GetDefaultMatchEvents(ClubType sport) => sport switch
    {
        ClubType.Cricket => "[\"Wicket\",\"Boundary\",\"Six\",\"RunOut\",\"Catch\",\"LBW\"]",
        ClubType.Football => "[\"Goal\",\"YellowCard\",\"RedCard\",\"Substitution\",\"Penalty\"]",
        ClubType.Hockey => "[\"Goal\",\"GreenCard\",\"YellowCard\",\"RedCard\",\"PenaltyCorner\"]",
        _ => "[\"Goal\",\"YellowCard\",\"RedCard\",\"Substitution\"]"
    };

    private static string GetScoreFields(ClubType sport) => sport switch
    {
        ClubType.Cricket => "{\"fields\":[\"runs\",\"wickets\",\"overs\"]}",
        ClubType.Football => "{\"fields\":[\"goals\"]}",
        ClubType.Hockey => "{\"fields\":[\"goals\"]}",
        ClubType.Tennis => "{\"fields\":[\"sets\",\"games\",\"points\"]}",
        ClubType.Rugby => "{\"fields\":[\"tries\",\"conversions\",\"penalties\"]}",
        _ => "{\"fields\":[\"score\"]}"
    };
}
