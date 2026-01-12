namespace HydraEnergyAPI.Models.DTOs;

public class ForecastResult
{
    public DateTime Date { get; set; }
    public double PredictedKwh { get; set; }
    public double ConfidenceLower { get; set; }
    public double ConfidenceUpper { get; set; }
    public string Method { get; set; } = "Linear Trend + Moving Average";
}

public class ForecastSummary
{
    public List<ForecastResult> Forecasts { get; set; } = new();
    public double AverageHistoricalConsumption { get; set; }
    public string TrendDirection { get; set; } = string.Empty;
    public double TrendStrength { get; set; }
}
