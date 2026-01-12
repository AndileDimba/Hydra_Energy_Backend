using HydraEnergyAPI.Configuration;
using HydraEnergyAPI.Models.DTOs;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HydraEnergyAPI.Services;

public class EnergyDataService : IEnergyDataService
{
    private readonly HttpClient _httpClient;
    private readonly HydraSettings _settings;
    private readonly IHydraAuthService _authService;
    private readonly ILogger<EnergyDataService> _logger;

    public EnergyDataService(
        HttpClient httpClient,
        IOptions<HydraSettings> settings,
        IHydraAuthService authService,
        ILogger<EnergyDataService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _authService = authService;
        _logger = logger;
    }

    public async Task<List<EnergyDataResponse>> GetEnergyDataAsync(DateTime fromDate, DateTime toDate)
    {
        return await GetEnergyDataAsync(
            fromDate.ToString("yyyy-MM-dd"),
            toDate.ToString("yyyy-MM-dd"));
    }

    public async Task<List<EnergyDataResponse>> GetEnergyDataAsync(string fromDateStr, string toDateStr)
    {
        try
        {
            _logger.LogInformation("Fetching energy data from {From} to {To}", fromDateStr, toDateStr);

            // Get access token
            var accessToken = await _authService.GetAccessTokenAsync();
            _logger.LogInformation("Access token obtained: {Token}", accessToken.Substring(0, Math.Min(50, accessToken.Length)) + "...");

            // Prepare request
            var request = new EnergyDataRequest
            {
                UseCsv = false,
                DeviceId = _settings.DeviceId,
                From = fromDateStr,
                To = toDateStr,
                Sensors = new List<string> { _settings.SensorId }
            };

            var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
            _logger.LogInformation("Request payload:\n{Payload}", jsonContent);

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Create request message with authorization header
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, _settings.ApiUrl)
            {
                Content = content
            };
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            _logger.LogInformation("Sending request to: {Url}", _settings.ApiUrl);

            // Send request
            var response = await _httpClient.SendAsync(requestMessage);

            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Response Status: {StatusCode}", response.StatusCode);
            _logger.LogInformation("Response Content: {Content}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch energy data: {StatusCode} - {Error}",
                    response.StatusCode, responseContent);
                throw new HttpRequestException($"Failed to fetch energy data: {response.StatusCode} - {responseContent}");
            }

            var energyData = JsonSerializer.Deserialize<List<EnergyDataResponse>>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (energyData == null)
            {
                _logger.LogWarning("No energy data returned from API");
                return new List<EnergyDataResponse>();
            }

            _logger.LogInformation("Successfully fetched {Count} energy data records", energyData.Count);
            return energyData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching energy data");
            throw;
        }
    }

    public async Task<double> GetTotalConsumptionAsync(DateTime fromDate, DateTime toDate)
    {
        var energyData = await GetEnergyDataAsync(fromDate, toDate);
        return energyData.Sum(d => d.KwhConsumption);
    }

    public async Task<double> GetAverageDailyConsumptionAsync(DateTime fromDate, DateTime toDate)
    {
        var energyData = await GetEnergyDataAsync(fromDate, toDate);
        return energyData.Any() ? energyData.Average(d => d.KwhConsumption) : 0;
    }
}