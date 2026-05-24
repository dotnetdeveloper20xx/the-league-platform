using TheLeague.Modules.Competitions.Domain;

namespace TheLeague.Modules.Competitions.Infrastructure.Services;

public class FixtureGenerator
{
    public List<Match> GenerateRoundRobin(Guid clubId, Guid competitionId, List<CompetitionTeam> teams, DateTime startDate)
    {
        if (teams.Count < 2)
            throw new InvalidOperationException("At least 2 teams are required to generate fixtures.");

        var matches = new List<Match>();
        int round = 1;
        var scheduledDate = startDate;

        for (int i = 0; i < teams.Count - 1; i++)
        {
            for (int j = i + 1; j < teams.Count; j++)
            {
                var match = Match.Create(
                    clubId,
                    competitionId,
                    teams[i].Id,
                    teams[j].Id,
                    scheduledDate,
                    roundNumber: round,
                    venueId: teams[i].HomeVenueId,
                    venueName: teams[i].HomeVenueName);

                matches.Add(match);
                scheduledDate = scheduledDate.AddDays(7);
                round++;
            }
        }

        return matches;
    }

    public List<Match> GenerateKnockout(Guid clubId, Guid competitionId, List<CompetitionTeam> teams, DateTime startDate)
    {
        if (teams.Count < 2)
            throw new InvalidOperationException("At least 2 teams are required to generate fixtures.");

        var matches = new List<Match>();
        var scheduledDate = startDate;

        // Find next power of 2 >= team count
        int bracketSize = 1;
        while (bracketSize < teams.Count)
            bracketSize *= 2;

        int byes = bracketSize - teams.Count;
        int round = 1;

        // Teams that get byes advance automatically; remaining teams play in round 1
        var teamsInFirstRound = teams.Skip(byes).ToList();

        for (int i = 0; i < teamsInFirstRound.Count; i += 2)
        {
            var match = Match.Create(
                clubId,
                competitionId,
                teamsInFirstRound[i].Id,
                teamsInFirstRound[i + 1].Id,
                scheduledDate,
                roundNumber: round);

            matches.Add(match);
            scheduledDate = scheduledDate.AddDays(7);
        }

        return matches;
    }
}
