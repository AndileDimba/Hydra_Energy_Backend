using HydraEnergyAPI.Models.DTOs;

namespace HydraEnergyAPI.Services;

public interface IForecastingService
{
    Task<ForecastSummary> ForecastEnergyConsumptionAsync(DateTime fromDate, int daysToForecast = 3);
    Task<List<ForecastResult>> GenerateForecastAsync(List<EnergyDataResponse> historicalData, int daysToForecast = 3);
}
