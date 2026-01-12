using System.Text.Json.Serialization;

namespace HydraEnergyAPI.Models.DTOs;

public class EnergyDataResponse
{
    [JsonPropertyName("sensorId")]
    public string SensorId { get; set; } = string.Empty;
    
    [JsonPropertyName("year")]
    public int Year { get; set; }
    
    [JsonPropertyName("month")]
    public int Month { get; set; }
    
    [JsonPropertyName("day")]
    public int Day { get; set; }
    
    [JsonPropertyName("count")]
    public int Count { get; set; }
    
    [JsonPropertyName("sum")]
    public double Sum { get; set; }
    
    [JsonPropertyName("min")]
    public double Min { get; set; }
    
    [JsonPropertyName("max")]
    public double Max { get; set; }
    
    public DateTime Date => new DateTime(Year, Month, Day);
    
    /// <summary>
    /// Calculated kWh consumption: (max - min)
    /// </summary>
    public double KwhConsumption => (Max - Min) / 1000; // Convert to kWh
}
