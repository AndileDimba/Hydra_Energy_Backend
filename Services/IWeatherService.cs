using HydraEnergyAPI.Models.DTOs;

namespace HydraEnergyAPI.Services;

public interface IWeatherService
{
    Task<List<WeatherData>> GetHistoricalWeatherAsync(DateTime fromDate, DateTime toDate);
    Task<WeatherData?> GetWeatherForDateAsync(DateTime date);
    Task<Dictionary<DateTime, WeatherData>> GetWeatherDictionaryAsync(DateTime fromDate, DateTime toDate);
}
