using Microsoft.Extensions.Logging;

namespace TheLeague.Shared.Infrastructure.Integrations;

public class MockGoCardlessService : IGoCardlessService
{
    private readonly ILogger<MockGoCardlessService> _logger;

    public MockGoCardlessService(ILogger<MockGoCardlessService> logger)
    {
        _logger = logger;
    }

    public Task<string> CreateMandateAsync(Guid memberId, string email, CancellationToken ct = default)
    {
        var mandateId = $"MD-MOCK-{Guid.NewGuid():N}";
        _logger.LogInformation("Mock GoCardless: Created mandate {MandateId} for member {MemberId} ({Email})", mandateId, memberId, email);
        return Task.FromResult(mandateId);
    }

    public Task<bool> CollectPaymentAsync(string mandateId, decimal amount, string description, CancellationToken ct = default)
    {
        _logger.LogInformation("Mock GoCardless: Collecting £{Amount} from mandate {MandateId} - {Description}", amount, mandateId, description);
        return Task.FromResult(true);
    }

    public Task CancelMandateAsync(string mandateId, CancellationToken ct = default)
    {
        _logger.LogInformation("Mock GoCardless: Cancelled mandate {MandateId}", mandateId);
        return Task.CompletedTask;
    }
}
