using HydraEnergyAPI.Models.DTOs;

namespace HydraEnergyAPI.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IEnergyDataService _energyDataService;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        IEnergyDataService energyDataService,
        ILogger<AnalyticsService> logger)
    {
        _energyDataService = energyDataService;
        _logger = logger;
    }

    public async Task<AnalyticsSummary> AnalyzeEnergyDataAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            _logger.LogInformation("Analyzing energy data from {From} to {To}", fromDate, toDate);

            // Fetch energy data
            var energyData = await _energyDataService.GetEnergyDataAsync(fromDate, toDate);

            if (!energyData.Any())
            {
                _logger.LogWarning("No energy data available for analysis");
                return new AnalyticsSummary
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    TotalEnergyUsed = 0,
                    AverageDailyUse = 0,
                    NumberOfAnomalies = 0,
                    DailyResults = new List<AnalyticsResult>()
                };
            }

            // Calculate analytics
            var results = await CalculateMovingAverageAndAnomaliesAsync(energyData);

            var summary = new AnalyticsSummary
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalEnergyUsed = energyData.Sum(d => d.KwhConsumption),
                AverageDailyUse = energyData.Average(d => d.KwhConsumption),
                NumberOfAnomalies = results.Count(r => r.IsAnomaly),
                DailyResults = results
            };

            _logger.LogInformation("Analysis complete: {Anomalies} anomalies detected out of {TotalDays} days",
                summary.NumberOfAnomalies, results.Count);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing energy data");
            throw;
        }
    }

    public async Task<List<AnalyticsResult>> CalculateMovingAverageAsync(
        List<EnergyDataResponse> energyData, int windowSize = 7)
    {
        var results = new List<AnalyticsResult>();
        var sortedData = energyData.OrderBy(d => d.Date).ToList();

        for (int i = 0; i < sortedData.Count; i++)
        {
            var current = sortedData[i];
            double? movingAverage = null;

            // Calculate moving average if we have enough data points
            if (i >= windowSize - 1)
            {
                var window = sortedData.Skip(i - windowSize + 1).Take(windowSize);
                movingAverage = window.Average(d => d.KwhConsumption);
            }

            results.Add(new AnalyticsResult
            {
                Date = current.Date,
                KwhConsumption = current.KwhConsumption,
                MovingAverage7Day = movingAverage,
                IsAnomaly = false
            });
        }

        return await Task.FromResult(results);
    }

    public async Task<List<AnalyticsResult>> DetectAnomaliesAsync(
        List<EnergyDataResponse> energyData, double threshold = 1.5)
    {
        var results = await CalculateMovingAverageAsync(energyData);

        // Calculate standard deviation for anomaly detection
        var consumptionValues = energyData.Select(d => d.KwhConsumption).ToList();
        var mean = consumptionValues.Average();
        var variance = consumptionValues.Average(v => Math.Pow(v - mean, 2));
        var stdDev = Math.Sqrt(variance);

        _logger.LogInformation("Mean consumption: {Mean:F2} kWh, Std Dev: {StdDev:F2} kWh", mean, stdDev);

        // Mark anomalies
        foreach (var result in results)
        {
            if (result.MovingAverage7Day.HasValue)
            {
                var deviation = result.KwhConsumption - result.MovingAverage7Day.Value;
                result.DeviationFromAverage = deviation;

                // Flag as anomaly if consumption is significantly above the moving average
                // Using threshold * stdDev as the criterion
                if (Math.Abs(deviation) > threshold * stdDev)
                {
                    result.IsAnomaly = true;
                    result.AnomalyReason = deviation > 0
                        ? $"Consumption {deviation:F2} kWh above 7-day average"
                        : $"Consumption {Math.Abs(deviation):F2} kWh below 7-day average";
                }
            }
        }

        return results;
    }

    private async Task<List<AnalyticsResult>> CalculateMovingAverageAndAnomaliesAsync(
        List<EnergyDataResponse> energyData)
    {
        // Calculate moving average first
        var results = await CalculateMovingAverageAsync(energyData, windowSize: 7);

        // Calculate statistics for anomaly detection
        var consumptionValues = energyData.Select(d => d.KwhConsumption).ToList();
        var mean = consumptionValues.Average();
        var variance = consumptionValues.Average(v => Math.Pow(v - mean, 2));
        var stdDev = Math.Sqrt(variance);

        // Detect anomalies using standard deviation method
        var threshold = 1.5; // 1.5 standard deviations

        foreach (var result in results)
        {
            if (result.MovingAverage7Day.HasValue)
            {
                var deviation = result.KwhConsumption - result.MovingAverage7Day.Value;
                result.DeviationFromAverage = deviation;

                // Flag as anomaly if consumption deviates significantly
                if (Math.Abs(deviation) > threshold * stdDev)
                {
                    result.IsAnomaly = true;
                    result.AnomalyReason = deviation > 0
                        ? $"High consumption: {deviation:F2} kWh above 7-day average ({result.MovingAverage7Day.Value:F2} kWh)"
                        : $"Low consumption: {Math.Abs(deviation):F2} kWh below 7-day average ({result.MovingAverage7Day.Value:F2} kWh)";
                    
                    _logger.LogInformation("Anomaly detected on {Date}: {Reason}",
                        result.Date.ToString("yyyy-MM-dd"), result.AnomalyReason);
                }
            }
        }

        return results;
    }
}
