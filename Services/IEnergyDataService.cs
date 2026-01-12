using HydraEnergyAPI.Models.DTOs;

namespace HydraEnergyAPI.Services;

public interface IEnergyDataService
{
    Task<List<EnergyDataResponse>> GetEnergyDataAsync(DateTime fromDate, DateTime toDate);
    Task<List<EnergyDataResponse>> GetEnergyDataAsync(string fromDateStr, string toDateStr);
    Task<double> GetTotalConsumptionAsync(DateTime fromDate, DateTime toDate);
    Task<double> GetAverageDailyConsumptionAsync(DateTime fromDate, DateTime toDate);
}
