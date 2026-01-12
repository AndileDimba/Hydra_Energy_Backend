using System.Text.Json.Serialization;

namespace HydraEnergyAPI.Models.DTOs;

public class WeatherData
{
    public DateTime Date { get; set; }
    public double Temperature { get; set; }
    public double FeelsLike { get; set; }
    public double TempMin { get; set; }
    public double TempMax { get; set; }
    public int Humidity { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Main { get; set; } = string.Empty;
}

public class OpenWeatherMapResponse
{
    [JsonPropertyName("list")]
    public List<OpenWeatherMapItem> List { get; set; } = new();
}

public class OpenWeatherMapItem
{
    [JsonPropertyName("dt")]
    public long Dt { get; set; }
    
    [JsonPropertyName("main")]
    public OpenWeatherMapMain Main { get; set; } = new();
    
    [JsonPropertyName("weather")]
    public List<OpenWeatherMapWeather> Weather { get; set; } = new();
    
    [JsonPropertyName("dt_txt")]
    public string DtTxt { get; set; } = string.Empty;
}

public class OpenWeatherMapMain
{
    [JsonPropertyName("temp")]
    public double Temp { get; set; }
    
    [JsonPropertyName("feels_like")]
    public double FeelsLike { get; set; }
    
    [JsonPropertyName("temp_min")]
    public double TempMin { get; set; }
    
    [JsonPropertyName("temp_max")]
    public double TempMax { get; set; }
    
    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }
}

public class OpenWeatherMapWeather
{
    [JsonPropertyName("main")]
    public string Main { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}
