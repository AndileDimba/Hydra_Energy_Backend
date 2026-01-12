namespace HydraEnergyAPI.Configuration;

public class HydraSettings
{
    public string AuthUrl { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string GrantType { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string SensorId { get; set; } = string.Empty;
}
