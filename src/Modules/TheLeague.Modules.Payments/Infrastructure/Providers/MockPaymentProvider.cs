namespace TheLeague.Modules.Payments.Infrastructure.Providers;

public class MockPaymentProvider : IPaymentProvider
{
    private readonly int _delayMs;
    private readonly double _failureRate;
    private readonly Random _random = new();

    public MockPaymentProvider(int delayMs = 500, double failureRate = 0.05)
    {
        _delayMs = delayMs;
        _failureRate = failureRate;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(ProcessPaymentRequest request, CancellationToken ct)
    {
        await Task.Delay(_delayMs, ct);

        if (_random.NextDouble() < _failureRate)
        {
            return new PaymentResult(
                Success: false,
                TransactionId: null,
                ErrorMessage: "Payment declined by provider (simulated failure)");
        }

        var transactionId = $"mock_txn_{Guid.NewGuid():N}";
        return new PaymentResult(
            Success: true,
            TransactionId: transactionId,
            ErrorMessage: null);
    }

    public async Task<RefundResult> ProcessRefundAsync(ProcessRefundRequest request, CancellationToken ct)
    {
        await Task.Delay(_delayMs, ct);

        if (_random.NextDouble() < _failureRate)
        {
            return new RefundResult(
                Success: false,
                RefundTransactionId: null,
                ErrorMessage: "Refund declined by provider (simulated failure)");
        }

        var refundTransactionId = $"mock_refund_{Guid.NewGuid():N}";
        return new RefundResult(
            Success: true,
            RefundTransactionId: refundTransactionId,
            ErrorMessage: null);
    }
}
