using HydraEnergyAPI.Models.DTOs;

namespace HydraEnergyAPI.Services;

public class ForecastingService : IForecastingService
{
    private readonly IEnergyDataService _energyDataService;
    private readonly ILogger<ForecastingService> _logger;

    public ForecastingService(
        IEnergyDataService energyDataService,
        ILogger<ForecastingService> logger)
    {
        _energyDataService = energyDataService;
        _logger = logger;
    }

    public async Task<ForecastSummary> ForecastEnergyConsumptionAsync(DateTime fromDate, int daysToForecast = 3)
    {
        try
        {
            _logger.LogInformation("Generating {Days}-day forecast starting from {Date}",
                daysToForecast, fromDate);

            // Fetch historical data (last 30 days for better trend analysis)
            var historicalStartDate = fromDate.AddDays(-30);
            var historicalData = await _energyDataService.GetEnergyDataAsync(historicalStartDate, fromDate.AddDays(-1));

            if (!historicalData.Any())
            {
                _logger.LogWarning("No historical data available for forecasting");
                return new ForecastSummary
                {
                    Forecasts = new List<ForecastResult>(),
                    AverageHistoricalConsumption = 0,
                    TrendDirection = "Unknown",
                    TrendStrength = 0
                };
            }

            // Generate forecast
            var forecasts = await GenerateForecastAsync(historicalData, daysToForecast);

            // Calculate trend
            var (trendDirection, trendStrength) = CalculateTrend(historicalData);

            var summary = new ForecastSummary
            {
                Forecasts = forecasts,
                AverageHistoricalConsumption = historicalData.Average(d => d.KwhConsumption),
                TrendDirection = trendDirection,
                TrendStrength = trendStrength
            };

            _logger.LogInformation("Forecast generated: Trend {Direction} with strength {Strength:F2}",
                trendDirection, trendStrength);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating forecast");
            throw;
        }
    }

    public async Task<List<ForecastResult>> GenerateForecastAsync(
        List<EnergyDataResponse> historicalData, int daysToForecast = 3)
    {
        var forecasts = new List<ForecastResult>();
        var sortedData = historicalData.OrderBy(d => d.Date).ToList();

        if (!sortedData.Any())
        {
            return forecasts;
        }

        // Calculate moving average and trend
        var recentData = sortedData.TakeLast(14).ToList(); // Last 14 days
        var movingAverage = recentData.Average(d => d.KwhConsumption);

        // Calculate linear trend using simple linear regression
        var (slope, intercept) = CalculateLinearRegression(recentData);

        // Calculate standard deviation for confidence intervals
        var stdDev = CalculateStandardDeviation(recentData.Select(d => d.KwhConsumption).ToList());

        // Generate forecasts
        var lastDate = sortedData.Last().Date;
        var lastDataPoint = recentData.Count;

        for (int i = 1; i <= daysToForecast; i++)
        {
            var forecastDate = lastDate.AddDays(i);
            var x = lastDataPoint + i;

            // Predicted value using linear trend
            var trendValue = slope * x + intercept;

            // Combine with moving average for more stable prediction
            var predictedKwh = (trendValue * 0.6) + (movingAverage * 0.4);

            // Ensure prediction is not negative
            predictedKwh = Math.Max(0, predictedKwh);

            // Calculate confidence intervals (±2 standard deviations for ~95% confidence)
            var confidenceMargin = 2 * stdDev;

            forecasts.Add(new ForecastResult
            {
                Date = forecastDate,
                PredictedKwh = predictedKwh,
                ConfidenceLower = Math.Max(0, predictedKwh - confidenceMargin),
                ConfidenceUpper = predictedKwh + confidenceMargin,
                Method = "Linear Trend (60%) + Moving Average (40%)"
            });
        }

        return await Task.FromResult(forecasts);
    }

    private (string direction, double strength) CalculateTrend(List<EnergyDataResponse> data)
    {
        if (data.Count < 2)
        {
            return ("Unknown", 0);
        }

        var sortedData = data.OrderBy(d => d.Date).ToList();
        var (slope, _) = CalculateLinearRegression(sortedData);

        var avgConsumption = sortedData.Average(d => d.KwhConsumption);
        var trendStrength = Math.Abs(slope) / avgConsumption * 100; // Percentage

        string direction;
        if (Math.Abs(slope) < avgConsumption * 0.01) // Less than 1% change
        {
            direction = "Stable";
        }
        else if (slope > 0)
        {
            direction = "Increasing";
        }
        else
        {
            direction = "Decreasing";
        }

        return (direction, trendStrength);
    }

    private (double slope, double intercept) CalculateLinearRegression(List<EnergyDataResponse> data)
    {
        var n = data.Count;
        if (n < 2)
        {
            return (0, data.FirstOrDefault()?.KwhConsumption ?? 0);
        }

        // Assign x values (0, 1, 2, ..., n-1)
        var sumX = 0.0;
        var sumY = 0.0;
        var sumXY = 0.0;
        var sumXX = 0.0;

        for (int i = 0; i < n; i++)
        {
            var x = i;
            var y = data[i].KwhConsumption;

            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumXX += x * x;
        }

        // Calculate slope and intercept
        var slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX);
        var intercept = (sumY - slope * sumX) / n;

        return (slope, intercept);
    }

    private double CalculateStandardDeviation(List<double> values)
    {
        if (!values.Any())
        {
            return 0;
        }

        var mean = values.Average();
        var variance = values.Average(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(variance);
    }
}
