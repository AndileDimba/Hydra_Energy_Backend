using HydraEnergyAPI.Configuration;
using HydraEnergyAPI.Models.DTOs;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HydraEnergyAPI.Services;

public class HydraAuthService : IHydraAuthService
{
    private readonly HttpClient _httpClient;
    private readonly HydraSettings _settings;
    private readonly ILogger<HydraAuthService> _logger;
    private HydraAuthResponse? _cachedToken;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    public HydraAuthService(
        HttpClient httpClient,
        IOptions<HydraSettings> settings,
        ILogger<HydraAuthService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        await _tokenLock.WaitAsync();
        try
        {
            // Check if we have a valid cached token
            if (_cachedToken != null && _cachedToken.ExpiresAt > DateTime.UtcNow.AddMinutes(5))
            {
                _logger.LogInformation("Using cached access token");
                return _cachedToken.AccessToken;
            }

            // Get a new token
            _logger.LogInformation("Fetching new access token from HYDRA");
            _cachedToken = await AuthenticateAsync();
            return _cachedToken.AccessToken;
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    public async Task<HydraAuthResponse> AuthenticateAsync()
    {
        try
        {
            var requestData = new Dictionary<string, string>
            {
                { "client_id", _settings.ClientId },
                { "grant_type", _settings.GrantType },
                { "client_secret", _settings.ClientSecret },
                { "scope", _settings.Scope },
                { "username", _settings.Username },
                { "password", _settings.Password }
            };

            var content = new FormUrlEncodedContent(requestData);
            var response = await _httpClient.PostAsync(_settings.AuthUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Authentication failed: {StatusCode} - {Error}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"Authentication failed: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<HydraAuthResponse>(responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (authResponse == null)
            {
                throw new InvalidOperationException("Failed to deserialize authentication response");
            }

            // Set the expiration time
            authResponse.ExpiresAt = DateTime.UtcNow.AddSeconds(authResponse.ExpiresIn);

            _logger.LogInformation("Successfully authenticated with HYDRA. Token expires at {ExpiresAt}",
                authResponse.ExpiresAt);

            return authResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication");
            throw;
        }
    }

    public async Task<bool> ValidateTokenAsync()
    {
        try
        {
            var token = await GetAccessTokenAsync();
            return !string.IsNullOrEmpty(token);
        }
        catch
        {
            return false;
        }
    }
}
