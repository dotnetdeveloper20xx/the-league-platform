namespace TheLeague.Modules.Analytics.Infrastructure.Services;

public interface IRevenueForecaster
{
    RevenueForecastResult Forecast(RevenueForecastInput input);
}

public record RevenueForecastInput(
    int ActiveMembershipCount,
    decimal AverageMonthlyFee,
    decimal HistoricalRenewalRate,
    List<decimal> Last12MonthsRevenue
);

public record MonthForecast(
    int MonthOffset,
    decimal ProjectedRevenue,
    decimal ProjectedMembershipRevenue,
    decimal ConfidenceLevel
);

public record RevenueForecastResult(
    List<MonthForecast> MonthlyForecasts,
    decimal TotalProjectedRevenue
);

public class RevenueForecaster : IRevenueForecaster
{
    /// <summary>
    /// Forecasts next 3 months based on active memberships, historical renewal rates (12 months),
    /// and historical payment data.
    /// </summary>
    public RevenueForecastResult Forecast(RevenueForecastInput input)
    {
        var forecasts = new List<MonthForecast>();
        var totalProjected = 0m;

        // Calculate average historical monthly revenue
        var historicalAverage = input.Last12MonthsRevenue.Count > 0
            ? input.Last12MonthsRevenue.Average()
            : 0m;

        // Calculate base membership revenue
        var baseMembershipRevenue = input.ActiveMembershipCount * input.AverageMonthlyFee;

        for (int month = 1; month <= 3; month++)
        {
            // Apply renewal rate decay for each month
            var renewalFactor = (decimal)Math.Pow((double)input.HistoricalRenewalRate, month);
            var projectedMembershipRevenue = Math.Round(baseMembershipRevenue * renewalFactor, 2);

            // Blend membership projection with historical average
            var blendedRevenue = historicalAverage > 0
                ? Math.Round((projectedMembershipRevenue * 0.6m) + (historicalAverage * 0.4m), 2)
                : projectedMembershipRevenue;

            // Confidence decreases with distance
            var confidence = Math.Round(100m - (month * 10m), 2);

            forecasts.Add(new MonthForecast(month, blendedRevenue, projectedMembershipRevenue, confidence));
            totalProjected += blendedRevenue;
        }

        return new RevenueForecastResult(forecasts, Math.Round(totalProjected, 2));
    }
}
