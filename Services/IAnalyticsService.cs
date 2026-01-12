using HydraEnergyAPI.Models.DTOs;

namespace HydraEnergyAPI.Services;

public interface IAnalyticsService
{
    Task<AnalyticsSummary> AnalyzeEnergyDataAsync(DateTime fromDate, DateTime toDate);
    Task<List<AnalyticsResult>> CalculateMovingAverageAsync(List<EnergyDataResponse> energyData, int windowSize = 7);
    Task<List<AnalyticsResult>> DetectAnomaliesAsync(List<EnergyDataResponse> energyData, double threshold = 1.5);
}
