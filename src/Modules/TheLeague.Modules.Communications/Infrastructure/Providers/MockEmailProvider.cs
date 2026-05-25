using Microsoft.Extensions.Logging;

namespace TheLeague.Modules.Communications.Infrastructure.Providers;

public class MockEmailProvider : IEmailProvider
{
    private readonly ILogger<MockEmailProvider> _logger;

    public MockEmailProvider(ILogger<MockEmailProvider> logger)
    {
        _logger = logger;
    }

    public Task<EmailSendResult> SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        _logger.LogInformation("Mock Email sent to {To} with subject '{Subject}'", to, subject);
        return Task.FromResult(new EmailSendResult(true));
    }
}
