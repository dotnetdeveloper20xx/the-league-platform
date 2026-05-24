namespace TheLeague.Modules.Payments.Infrastructure.Providers;

public record ProcessPaymentRequest(
    decimal Amount,
    string Currency,
    Guid MemberId,
    string? Description,
    string? ExternalAccountId);

public record PaymentResult(
    bool Success,
    string? TransactionId,
    string? ErrorMessage);

public record ProcessRefundRequest(
    string OriginalTransactionId,
    decimal Amount,
    string? Reason);

public record RefundResult(
    bool Success,
    string? RefundTransactionId,
    string? ErrorMessage);

public interface IPaymentProvider
{
    Task<PaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request, CancellationToken ct);
    Task<RefundResult> ProcessRefundAsync(ProcessRefundRequest request, CancellationToken ct);
}
