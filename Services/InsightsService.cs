using HydraEnergyAPI.Models.DTOs;

namespace HydraEnergyAPI.Services;

public class InsightsService : IInsightsService
{
    private readonly IEnergyDataService _energyDataService;
    private readonly IWeatherService _weatherService;
    private readonly IAnalyticsService _analyticsService;
    private readonly IForecastingService _forecastingService;
    private readonly ILogger<InsightsService> _logger;

    public InsightsService(
        IEnergyDataService energyDataService,
        IWeatherService weatherService,
        IAnalyticsService analyticsService,
        IForecastingService forecastingService,
        ILogger<InsightsService> logger)
    {
        _energyDataService = energyDataService;
        _weatherService = weatherService;
        _analyticsService = analyticsService;
        _forecastingService = forecastingService;
        _logger = logger;
    }

    public async Task<InsightsSummary> GenerateInsightsAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            _logger.LogInformation("Generating insights from {From} to {To}", fromDate, toDate);

            var insights = new List<InsightResult>();

            // Fetch data
            var energyData = await _energyDataService.GetEnergyDataAsync(fromDate, toDate);
            var weatherData = await _weatherService.GetWeatherDictionaryAsync(fromDate, toDate);
            var analytics = await _analyticsService.AnalyzeEnergyDataAsync(fromDate, toDate);
            var forecast = await _forecastingService.ForecastEnergyConsumptionAsync(toDate.AddDays(1), 3);

            if (!energyData.Any())
            {
                insights.Add(new InsightResult
                {
                    Type = "NoData",
                    Message = "No energy data available for the selected period.",
                    Severity = "warning"
                });

                return new InsightsSummary
                {
                    Insights = insights,
                    OverallAssessment = "Insufficient data for analysis.",
                    GeneratedAt = DateTime.UtcNow
                };
            }

            // Generate consumption insights
            insights.AddRange(GenerateConsumptionInsights(energyData, analytics));

            // Generate anomaly insights
            insights.AddRange(GenerateAnomalyInsights(analytics));

            // Generate weather correlation insights
            if (weatherData.Any())
            {
                insights.AddRange(await GenerateWeatherCorrelationInsightsAsync(energyData, weatherData));
            }

            // Generate trend insights
            insights.AddRange(GenerateTrendInsights(energyData, forecast));

            // Generate forecast insights
            insights.AddRange(GenerateForecastInsights(forecast, analytics));

            // Generate overall assessment
            var overallAssessment = GenerateOverallAssessment(analytics, forecast, insights);

            var summary = new InsightsSummary
            {
                Insights = insights,
                OverallAssessment = overallAssessment,
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Generated {Count} insights", insights.Count);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating insights");
            throw;
        }
    }

    private List<InsightResult> GenerateConsumptionInsights(
        List<EnergyDataResponse> energyData,
        AnalyticsSummary analytics)
    {
        var insights = new List<InsightResult>();

        // Total consumption insight
        insights.Add(new InsightResult
        {
            Type = "TotalConsumption",
            Message = $"Total energy consumption for the period: {analytics.TotalEnergyUsed:F2} kWh over {energyData.Count} days.",
            Severity = "info",
            Metadata = new Dictionary<string, object>
            {
                { "totalKwh", analytics.TotalEnergyUsed },
                { "days", energyData.Count }
            }
        });

        // Average daily consumption insight
        insights.Add(new InsightResult
        {
            Type = "AverageConsumption",
            Message = $"Average daily energy consumption: {analytics.AverageDailyUse:F2} kWh.",
            Severity = "info",
            Metadata = new Dictionary<string, object>
            {
                { "avgKwh", analytics.AverageDailyUse }
            }
        });

        // Peak consumption day
        var peakDay = energyData.OrderByDescending(d => d.KwhConsumption).First();
        insights.Add(new InsightResult
        {
            Type = "PeakConsumption",
            Message = $"Peak consumption occurred on {peakDay.Date:yyyy-MM-dd} with {peakDay.KwhConsumption:F2} kWh, which is {((peakDay.KwhConsumption / analytics.AverageDailyUse - 1) * 100):F1}% above average.",
            Severity = "info",
            RelatedDate = peakDay.Date,
            Metadata = new Dictionary<string, object>
            {
                { "peakKwh", peakDay.KwhConsumption },
                { "date", peakDay.Date }
            }
        });

        // Lowest consumption day
        var lowestDay = energyData.OrderBy(d => d.KwhConsumption).First();
        insights.Add(new InsightResult
        {
            Type = "LowestConsumption",
            Message = $"Lowest consumption occurred on {lowestDay.Date:yyyy-MM-dd} with {lowestDay.KwhConsumption:F2} kWh.",
            Severity = "info",
            RelatedDate = lowestDay.Date,
            Metadata = new Dictionary<string, object>
            {
                { "lowestKwh", lowestDay.KwhConsumption },
                { "date", lowestDay.Date }
            }
        });

        return insights;
    }

    private List<InsightResult> GenerateAnomalyInsights(AnalyticsSummary analytics)
    {
        var insights = new List<InsightResult>();

        if (analytics.NumberOfAnomalies == 0)
        {
            insights.Add(new InsightResult
            {
                Type = "NoAnomalies",
                Message = "No unusual consumption patterns detected. Energy usage has been consistent.",
                Severity = "info"
            });
        }
        else
        {
            insights.Add(new InsightResult
            {
                Type = "AnomalyDetected",
                Message = $"{analytics.NumberOfAnomalies} day(s) with unusual consumption patterns detected.",
                Severity = analytics.NumberOfAnomalies > 3 ? "warning" : "info",
                Metadata = new Dictionary<string, object>
                {
                    { "anomalyCount", analytics.NumberOfAnomalies }
                }
            });

            // Detail the most significant anomaly
            var significantAnomaly = analytics.DailyResults
                .Where(r => r.IsAnomaly)
                .OrderByDescending(r => Math.Abs(r.DeviationFromAverage ?? 0))
                .FirstOrDefault();

            if (significantAnomaly != null)
            {
                insights.Add(new InsightResult
                {
                    Type = "SignificantAnomaly",
                    Message = $"Most significant anomaly on {significantAnomaly.Date:yyyy-MM-dd}: {significantAnomaly.AnomalyReason}",
                    Severity = "warning",
                    RelatedDate = significantAnomaly.Date,
                    Metadata = new Dictionary<string, object>
                    {
                        { "deviation", significantAnomaly.DeviationFromAverage ?? 0 },
                        { "consumption", significantAnomaly.KwhConsumption }
                    }
                });
            }
        }

        return insights;
    }

    public async Task<List<InsightResult>> GenerateWeatherCorrelationInsightsAsync(
        List<EnergyDataResponse> energyData,
        Dictionary<DateTime, WeatherData> weatherData)
    {
        var insights = new List<InsightResult>();

        try
        {
            // Find correlation between temperature and energy consumption
            var correlatedData = energyData
                .Where(e => weatherData.ContainsKey(e.Date.Date))
                .Select(e => new
                {
                    Energy = e,
                    Weather = weatherData[e.Date.Date]
                })
                .ToList();

            if (correlatedData.Any())
            {
                // Find hot days with high consumption
                var hotDaysHighConsumption = correlatedData
                    .Where(d => d.Weather.Temperature > 28 && 
                                d.Energy.KwhConsumption > energyData.Average(e => e.KwhConsumption))
                    .ToList();

                if (hotDaysHighConsumption.Any())
                {
                    var avgTempHotDays = hotDaysHighConsumption.Average(d => d.Weather.Temperature);
                    var avgConsumptionHotDays = hotDaysHighConsumption.Average(d => d.Energy.KwhConsumption);
                    var avgConsumptionOverall = energyData.Average(e => e.KwhConsumption);
                    var increase = ((avgConsumptionHotDays / avgConsumptionOverall - 1) * 100);

                    insights.Add(new InsightResult
                    {
                        Type = "WeatherImpact",
                        Message = $"Energy consumption increased by {increase:F1}% on hot days (avg {avgTempHotDays:F1}°C) compared to overall average, likely due to increased cooling demand.",
                        Severity = "info",
                        Metadata = new Dictionary<string, object>
                        {
                            { "avgTempHotDays", avgTempHotDays },
                            { "increasePercent", increase },
                            { "daysCount", hotDaysHighConsumption.Count }
                        }
                    });
                }

                // Find rainy days correlation
                var rainyDays = correlatedData
                    .Where(d => d.Weather.Main.Contains("Rain", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (rainyDays.Any())
                {
                    var avgConsumptionRainyDays = rainyDays.Average(d => d.Energy.KwhConsumption);
                    var avgConsumptionOverall = energyData.Average(e => e.KwhConsumption);
                    var change = ((avgConsumptionRainyDays / avgConsumptionOverall - 1) * 100);

                    insights.Add(new InsightResult
                    {
                        Type = "WeatherPattern",
                        Message = $"On rainy days ({rainyDays.Count} days), energy consumption was {Math.Abs(change):F1}% {(change > 0 ? "higher" : "lower")} than average.",
                        Severity = "info",
                        Metadata = new Dictionary<string, object>
                        {
                            { "rainyDays", rainyDays.Count },
                            { "changePercent", change }
                        }
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating weather correlation insights");
        }

        return await Task.FromResult(insights);
    }

    private List<InsightResult> GenerateTrendInsights(
        List<EnergyDataResponse> energyData,
        ForecastSummary forecast)
    {
        var insights = new List<InsightResult>();

        var trendMessage = forecast.TrendDirection switch
        {
            "Increasing" => $"Energy consumption is trending upward with a {forecast.TrendStrength:F1}% increase rate. Consider investigating causes for rising consumption.",
            "Decreasing" => $"Energy consumption is trending downward with a {forecast.TrendStrength:F1}% decrease rate. This could indicate improved efficiency or reduced usage.",
            "Stable" => "Energy consumption has remained stable over the analyzed period.",
            _ => "Unable to determine consumption trend."
        };

        var severity = forecast.TrendDirection == "Increasing" && forecast.TrendStrength > 5 ? "warning" : "info";

        insights.Add(new InsightResult
        {
            Type = "ConsumptionTrend",
            Message = trendMessage,
            Severity = severity,
            Metadata = new Dictionary<string, object>
            {
                { "trend", forecast.TrendDirection },
                { "strength", forecast.TrendStrength }
            }
        });

        // Week-over-week comparison if we have enough data
        if (energyData.Count >= 14)
        {
            var sortedData = energyData.OrderBy(d => d.Date).ToList();
            var lastWeek = sortedData.TakeLast(7).Sum(d => d.KwhConsumption);
            var previousWeek = sortedData.Skip(sortedData.Count - 14).Take(7).Sum(d => d.KwhConsumption);
            var weekChange = ((lastWeek / previousWeek - 1) * 100);

            var weekMessage = weekChange > 0
                ? $"Last week's consumption increased by {weekChange:F1}% compared to the previous week."
                : $"Last week's consumption decreased by {Math.Abs(weekChange):F1}% compared to the previous week.";

            insights.Add(new InsightResult
            {
                Type = "WeeklyComparison",
                Message = weekMessage,
                Severity = Math.Abs(weekChange) > 15 ? "warning" : "info",
                Metadata = new Dictionary<string, object>
                {
                    { "lastWeekKwh", lastWeek },
                    { "previousWeekKwh", previousWeek },
                    { "changePercent", weekChange }
                }
            });
        }

        return insights;
    }

    private List<InsightResult> GenerateForecastInsights(
        ForecastSummary forecast,
        AnalyticsSummary analytics)
    {
        var insights = new List<InsightResult>();

        if (forecast.Forecasts.Any())
        {
            var avgForecast = forecast.Forecasts.Average(f => f.PredictedKwh);
            var change = ((avgForecast / analytics.AverageDailyUse - 1) * 100);

            var forecastMessage = change > 5
                ? $"Next 3 days forecast: Expected consumption {change:F1}% higher than recent average. Plan for increased energy demand."
                : change < -5
                    ? $"Next 3 days forecast: Expected consumption {Math.Abs(change):F1}% lower than recent average."
                    : "Next 3 days forecast: Expected consumption similar to recent average.";

            insights.Add(new InsightResult
            {
                Type = "ForecastPrediction",
                Message = forecastMessage,
                Severity = change > 10 ? "warning" : "info",
                Metadata = new Dictionary<string, object>
                {
                    { "avgForecastKwh", avgForecast },
                    { "changePercent", change }
                }
            });
        }

        return insights;
    }

    private string GenerateOverallAssessment(
        AnalyticsSummary analytics,
        ForecastSummary forecast,
        List<InsightResult> insights)
    {
        var criticalInsights = insights.Count(i => i.Severity == "critical");
        var warningInsights = insights.Count(i => i.Severity == "warning");

        if (criticalInsights > 0)
        {
            return $"⚠️ Critical attention required: {criticalInsights} critical issue(s) detected. Energy consumption shows significant anomalies that need immediate investigation.";
        }

        if (warningInsights > 2)
        {
            return $"⚠️ Monitor closely: {warningInsights} warning(s) detected. Energy consumption patterns show notable variations from expected behavior.";
        }

        if (forecast.TrendDirection == "Increasing" && forecast.TrendStrength > 10)
        {
            return $"📈 Rising consumption trend detected. Average daily use is {analytics.AverageDailyUse:F2} kWh with an increasing trajectory. Consider energy optimization strategies.";
        }

        if (forecast.TrendDirection == "Decreasing")
        {
            return $"📉 Positive trend: Energy consumption is decreasing. Average daily use is {analytics.AverageDailyUse:F2} kWh with improving efficiency.";
        }

        return $"✅ Normal operation: Energy consumption is stable at {analytics.AverageDailyUse:F2} kWh per day on average. {analytics.NumberOfAnomalies} anomaly(ies) detected over {analytics.DailyResults.Count} days.";
    }
}
