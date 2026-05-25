using Microsoft.Extensions.Logging;

namespace TheLeague.Shared.Infrastructure.Search;

public class SearchService : ISearchService
{
    private readonly ILogger<SearchService> _logger;

    public SearchService(ILogger<SearchService> logger)
    {
        _logger = logger;
    }

    public Task<SearchResults> SearchAsync(Guid clubId, string query, int maxResults = 20, CancellationToken ct = default)
    {
        _logger.LogInformation("Searching for '{Query}' in club {ClubId} (max {MaxResults} results)",
            query, clubId, maxResults);

        // Placeholder: Will be backed by SQL full-text search
        var result = new SearchResults(Items: new List<SearchResultItem>(), TotalCount: 0);

        return Task.FromResult(result);
    }
}
