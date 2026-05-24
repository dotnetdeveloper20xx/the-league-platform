namespace TheLeague.Modules.Payments.Infrastructure;

public class PlatformFeeCalculator
{
    private readonly decimal _feePercentage;
    private readonly decimal _minimumFee;

    /// <summary>
    /// Calculates platform fee for payment transactions.
    /// </summary>
    /// <param name="feePercentage">Fee percentage (1-2%, e.g. 1.5 means 1.5%)</param>
    /// <param name="minimumFee">Minimum fee amount (default £0.30)</param>
    public PlatformFeeCalculator(decimal feePercentage = 1.5m, decimal minimumFee = 0.30m)
    {
        if (feePercentage < 1m || feePercentage > 2m)
            throw new ArgumentOutOfRangeException(nameof(feePercentage), "Fee percentage must be between 1% and 2%.");

        _feePercentage = feePercentage;
        _minimumFee = minimumFee;
    }

    public decimal CalculateFee(decimal transactionAmount)
    {
        if (transactionAmount <= 0)
            return 0;

        var calculatedFee = Math.Round(transactionAmount * (_feePercentage / 100m), 2);
        return Math.Max(calculatedFee, _minimumFee);
    }
}
