using Microsoft.Extensions.Logging;

namespace TheLeague.Modules.Communications.Infrastructure.Providers;

public class MockSmsProvider : ISmsProvider
{
    private readonly ILogger<MockSmsProvider> _logger;

    public MockSmsProvider(ILogger<MockSmsProvider> logger)
    {
        _logger = logger;
    }

    public Task<SmsSendResult> SendAsync(string to, string message, CancellationToken ct = default)
    {
        _logger.LogInformation("Mock SMS sent to {To}: '{Message}'", to, message);
        return Task.FromResult(new SmsSendResult(true));
    }
}
