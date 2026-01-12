using System.Text.Json.Serialization;

namespace HydraEnergyAPI.Models.DTOs;

public class EnergyDataRequest
{
    [JsonPropertyName("useCsv")]
    public bool UseCsv { get; set; } = false;
    
    [JsonPropertyName("deviceId")]
    public string DeviceId { get; set; } = string.Empty;
    
    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;
    
    [JsonPropertyName("to")]
    public string To { get; set; } = string.Empty;
    
    [JsonPropertyName("sensors")]
    public List<string> Sensors { get; set; } = new();
}
