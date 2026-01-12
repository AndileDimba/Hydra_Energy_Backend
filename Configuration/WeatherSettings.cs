namespace HydraEnergyAPI.Configuration;

public class WeatherSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiUrl { get; set; } = string.Empty;
    public string City { get; set; } = "Johannesburg";
    public string CountryCode { get; set; } = "ZA";
    public double Latitude { get; set; } = -26.2041;
    public double Longitude { get; set; } = 28.0473;
}
