using TheLeague.Modules.Competitions.Domain;
using TheLeague.Shared.Domain.Enums;

namespace TheLeague.Modules.Competitions.Infrastructure.Services;

public class StandingsCalculator
{
    public List<CompetitionStanding> Calculate(
        Guid clubId,
        Guid competitionId,
        List<CompetitionTeam> teams,
        List<Match> completedMatches,
        int pointsForWin = 3,
        int pointsForDraw = 1,
        int pointsForLoss = 0)
    {
        var standings = new List<CompetitionStanding>();

        foreach (var team in teams)
        {
            int played = 0, won = 0, drawn = 0, lost = 0, goalsFor = 0, goalsAgainst = 0;

            var teamMatches = completedMatches
                .Where(m => (m.HomeTeamId == team.Id || m.AwayTeamId == team.Id)
                            && (m.Status == MatchStatus.Completed || m.Status == MatchStatus.Walkover))
                .OrderByDescending(m => m.ScheduledDateTime)
                .ToList();

            foreach (var match in teamMatches)
            {
                played++;
                bool isHome = match.HomeTeamId == team.Id;

                int homeGoals = ParseScore(match.HomeScore);
                int awayGoals = ParseScore(match.AwayScore);

                if (isHome)
                {
                    goalsFor += homeGoals;
                    goalsAgainst += awayGoals;
                }
                else
                {
                    goalsFor += awayGoals;
                    goalsAgainst += homeGoals;
                }

                var result = match.Result;
                if (result == MatchResult.Draw)
                {
                    drawn++;
                }
                else if ((isHome && (result == MatchResult.HomeWin || result == MatchResult.HomeWalkover)) ||
                         (!isHome && (result == MatchResult.AwayWin || result == MatchResult.AwayWalkover)))
                {
                    won++;
                }
                else
                {
                    lost++;
                }
            }

            int points = (won * pointsForWin) + (drawn * pointsForDraw) + (lost * pointsForLoss);

            // Form: last 5 results
            var form = string.Join("", teamMatches.Take(5).Select(m =>
            {
                bool isHome = m.HomeTeamId == team.Id;
                var result = m.Result;
                if (result == MatchResult.Draw) return "D";
                if ((isHome && (result == MatchResult.HomeWin || result == MatchResult.HomeWalkover)) ||
                    (!isHome && (result == MatchResult.AwayWin || result == MatchResult.AwayWalkover)))
                    return "W";
                return "L";
            }));

            var standing = CompetitionStanding.Create(clubId, competitionId, team.Id);
            standing.Update(played, won, drawn, lost, goalsFor, goalsAgainst, points, form.Length > 0 ? form : null);
            standings.Add(standing);
        }

        return standings.OrderByDescending(s => s.Points)
                       .ThenByDescending(s => s.GoalDifference)
                       .ThenByDescending(s => s.GoalsFor)
                       .ToList();
    }

    private static int ParseScore(string? score)
    {
        if (string.IsNullOrWhiteSpace(score))
            return 0;

        // Try to parse as simple integer first
        if (int.TryParse(score, out int simpleScore))
            return simpleScore;

        // Try to parse JSON format like {"goals": 2}
        // For simplicity, extract first number found
        var digits = new string(score.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out int parsed) ? parsed : 0;
    }
}
