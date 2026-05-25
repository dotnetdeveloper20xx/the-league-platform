namespace TheLeague.Shared.Infrastructure.Integrations;

public interface IGoCardlessService
{
    Task<string> CreateMandateAsync(Guid memberId, string email, CancellationToken ct = default);
    Task<bool> CollectPaymentAsync(string mandateId, decimal amount, string description, CancellationToken ct = default);
    Task CancelMandateAsync(string mandateId, CancellationToken ct = default);
}
