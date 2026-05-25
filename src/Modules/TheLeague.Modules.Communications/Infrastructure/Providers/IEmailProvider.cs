namespace TheLeague.Modules.Communications.Infrastructure.Providers;

public interface IEmailProvider
{
    Task<EmailSendResult> SendAsync(string to, string subject, string body, CancellationToken ct = default);
}

public record EmailSendResult(bool Success, string? ErrorMessage = null);
