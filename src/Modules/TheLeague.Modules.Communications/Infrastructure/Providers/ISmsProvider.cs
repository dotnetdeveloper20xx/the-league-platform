namespace TheLeague.Modules.Communications.Infrastructure.Providers;

public interface ISmsProvider
{
    Task<SmsSendResult> SendAsync(string to, string message, CancellationToken ct = default);
}

public record SmsSendResult(bool Success, string? ErrorMessage = null);
