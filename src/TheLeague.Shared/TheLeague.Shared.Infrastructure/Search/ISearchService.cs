namespace TheLeague.Shared.Infrastructure.Search;

public interface ISearchService
{
    Task<SearchResults> SearchAsync(Guid clubId, string query, int maxResults = 20, CancellationToken ct = default);
}

public record SearchResults(
    List<SearchResultItem> Items,
    int TotalCount);

public record SearchResultItem(
    string EntityType,
    Guid EntityId,
    string Title,
    string? Subtitle,
    string? MatchedText);
