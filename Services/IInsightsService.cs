using HydraEnergyAPI.Models.DTOs;

namespace HydraEnergyAPI.Services;

public interface IInsightsService
{
    Task<InsightsSummary> GenerateInsightsAsync(DateTime fromDate, DateTime toDate);
    Task<List<InsightResult>> GenerateWeatherCorrelationInsightsAsync(
        List<EnergyDataResponse> energyData,
        Dictionary<DateTime, WeatherData> weatherData);
}
