namespace HydraEnergyAPI.Models.DTOs;

public class AnalyticsResult
{
    public DateTime Date { get; set; }
    public double KwhConsumption { get; set; }
    public double? MovingAverage7Day { get; set; }
    public bool IsAnomaly { get; set; }
    public double? DeviationFromAverage { get; set; }
    public string? AnomalyReason { get; set; }
}

public class AnalyticsSummary
{
    public double TotalEnergyUsed { get; set; }
    public double AverageDailyUse { get; set; }
    public int NumberOfAnomalies { get; set; }
    public List<AnalyticsResult> DailyResults { get; set; } = new();
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}
