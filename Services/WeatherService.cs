using HydraEnergyAPI.Configuration;
using HydraEnergyAPI.Models.DTOs;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HydraEnergyAPI.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly WeatherSettings _settings;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        HttpClient httpClient,
        IOptions<WeatherSettings> settings,
        ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<List<WeatherData>> GetHistoricalWeatherAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            _logger.LogInformation("Fetching weather data from {From} to {To}", fromDate, toDate);

            // Note: OpenWeatherMap free tier doesn't provide historical data
            // For demo purposes, we'll fetch current/forecast data and simulate historical data
            // In production, you would use a paid API or historical weather service

            var weatherData = new List<WeatherData>();

            // Check if API key is set
            if (string.IsNullOrEmpty(_settings.ApiKey) || _settings.ApiKey == "YOUR_OPENWEATHERMAP_API_KEY")
            {
                _logger.LogWarning("OpenWeatherMap API key not configured. Returning simulated weather data.");
                return GenerateSimulatedWeatherData(fromDate, toDate);
            }

            try
            {
                // Try to fetch forecast data (5-day forecast available in free tier)
                var url = $"{_settings.ApiUrl}?lat={_settings.Latitude}&lon={_settings.Longitude}&appid={_settings.ApiKey}&units=metric";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var forecast = JsonSerializer.Deserialize<OpenWeatherMapResponse>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (forecast?.List != null)
                    {
                        // Group by day and take daily average
                        var dailyData = forecast.List
                            .GroupBy(item => DateTimeOffset.FromUnixTimeSeconds(item.Dt).Date)
                            .Select(g => new WeatherData
                            {
                                Date = g.Key,
                                Temperature = g.Average(x => x.Main.Temp),
                                FeelsLike = g.Average(x => x.Main.FeelsLike),
                                TempMin = g.Min(x => x.Main.TempMin),
                                TempMax = g.Max(x => x.Main.TempMax),
                                Humidity = (int)g.Average(x => x.Main.Humidity),
                                Description = g.First().Weather.FirstOrDefault()?.Description ?? "",
                                Main = g.First().Weather.FirstOrDefault()?.Main ?? ""
                            })
                            .Where(w => w.Date >= fromDate && w.Date <= toDate)
                            .ToList();

                        weatherData.AddRange(dailyData);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch weather data from API. Using simulated data.");
            }

            // If we don't have enough data, fill in with simulated data
            if (weatherData.Count == 0)
            {
                weatherData = GenerateSimulatedWeatherData(fromDate, toDate);
            }

            _logger.LogInformation("Returning {Count} weather records", weatherData.Count);
            return weatherData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data");
            // Return simulated data as fallback
            return GenerateSimulatedWeatherData(fromDate, toDate);
        }
    }

    public async Task<WeatherData?> GetWeatherForDateAsync(DateTime date)
    {
        var weatherList = await GetHistoricalWeatherAsync(date, date);
        return weatherList.FirstOrDefault();
    }

    public async Task<Dictionary<DateTime, WeatherData>> GetWeatherDictionaryAsync(DateTime fromDate, DateTime toDate)
    {
        var weatherList = await GetHistoricalWeatherAsync(fromDate, toDate);
        return weatherList.ToDictionary(w => w.Date.Date, w => w);
    }

    private List<WeatherData> GenerateSimulatedWeatherData(DateTime fromDate, DateTime toDate)
    {
        _logger.LogInformation("Generating simulated weather data for Johannesburg");

        var weatherData = new List<WeatherData>();
        var random = new Random();
        var currentDate = fromDate.Date;

        // Johannesburg typical summer temperatures (December)
        var baseTempMin = 15.0;
        var baseTempMax = 28.0;

        while (currentDate <= toDate.Date)
        {
            var tempVariation = random.NextDouble() * 8 - 4; // -4 to +4 degrees variation
            var tempMin = baseTempMin + tempVariation;
            var tempMax = baseTempMax + tempVariation;
            var avgTemp = (tempMin + tempMax) / 2;

            var conditions = new[] { "Clear", "Partly Cloudy", "Cloudy", "Rain", "Thunderstorm" };
            var weights = new[] { 0.4, 0.3, 0.15, 0.1, 0.05 }; // Higher chance of clear/partly cloudy
            var condition = WeightedRandomChoice(conditions, weights, random);

            weatherData.Add(new WeatherData
            {
                Date = currentDate,
                Temperature = avgTemp,
                FeelsLike = avgTemp + random.NextDouble() * 2,
                TempMin = tempMin,
                TempMax = tempMax,
                Humidity = random.Next(30, 70),
                Description = condition.ToLower(),
                Main = condition
            });

            currentDate = currentDate.AddDays(1);
        }

        return weatherData;
    }

    private T WeightedRandomChoice<T>(T[] items, double[] weights, Random random)
    {
        var totalWeight = weights.Sum();
        var randomValue = random.NextDouble() * totalWeight;
        var cumulative = 0.0;

        for (int i = 0; i < items.Length; i++)
        {
            cumulative += weights[i];
            if (randomValue < cumulative)
                return items[i];
        }

        return items[^1];
    }
}
