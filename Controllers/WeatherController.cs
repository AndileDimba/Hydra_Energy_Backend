using HydraEnergyAPI.Models.DTOs;
using HydraEnergyAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HydraEnergyAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(
        IWeatherService weatherService,
        ILogger<WeatherController> logger)
    {
        _weatherService = weatherService;
        _logger = logger;
    }

    /// <summary>
    /// Get weather data for a date range
    /// </summary>
    /// <param name="fromDate">Start date (yyyy-MM-dd)</param>
    /// <param name="toDate">End date (yyyy-MM-dd)</param>
    [HttpGet("data")]
    public async Task<ActionResult<ApiResponse<List<WeatherData>>>> GetWeatherData(
        [FromQuery] string fromDate,
        [FromQuery] string toDate)
    {
        try
        {
            if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate))
            {
                return BadRequest(new ApiResponse<List<WeatherData>>
                {
                    Success = false,
                    Message = "fromDate and toDate parameters are required",
                    Errors = new List<string> { "Missing required parameters" }
                });
            }

            var from = DateTime.Parse(fromDate);
            var to = DateTime.Parse(toDate);

            _logger.LogInformation("Fetching weather data from {From} to {To}", fromDate, toDate);
            var result = await _weatherService.GetHistoricalWeatherAsync(from, to);

            return Ok(new ApiResponse<List<WeatherData>>
            {
                Success = true,
                Data = result,
                Message = $"Retrieved {result.Count} weather records"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data");
            return StatusCode(500, new ApiResponse<List<WeatherData>>
            {
                Success = false,
                Message = "Error fetching weather data",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get weather data for a specific date
    /// </summary>
    /// <param name="date">Date (yyyy-MM-dd)</param>
    [HttpGet("date/{date}")]
    public async Task<ActionResult<ApiResponse<WeatherData>>> GetWeatherForDate(string date)
    {
        try
        {
            if (!DateTime.TryParse(date, out var parsedDate))
            {
                return BadRequest(new ApiResponse<WeatherData>
                {
                    Success = false,
                    Message = "Invalid date format. Use yyyy-MM-dd"
                });
            }

            var result = await _weatherService.GetWeatherForDateAsync(parsedDate);

            if (result == null)
            {
                return NotFound(new ApiResponse<WeatherData>
                {
                    Success = false,
                    Message = "No weather data found for the specified date"
                });
            }

            return Ok(new ApiResponse<WeatherData>
            {
                Success = true,
                Data = result,
                Message = "Weather data retrieved"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching weather data for date");
            return StatusCode(500, new ApiResponse<WeatherData>
            {
                Success = false,
                Message = "Error fetching weather data",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}
