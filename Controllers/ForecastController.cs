using HydraEnergyAPI.Models.DTOs;
using HydraEnergyAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HydraEnergyAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ForecastController : ControllerBase
{
    private readonly IForecastingService _forecastingService;
    private readonly ILogger<ForecastController> _logger;

    public ForecastController(
        IForecastingService forecastingService,
        ILogger<ForecastController> logger)
    {
        _forecastingService = forecastingService;
        _logger = logger;
    }

    /// <summary>
    /// Get energy consumption forecast for the next N days
    /// </summary>
    /// <param name="fromDate">Start date for forecast (yyyy-MM-dd)</param>
    /// <param name="days">Number of days to forecast (default: 3)</param>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<ForecastSummary>>> GetForecast(
        [FromQuery] string? fromDate = null,
        [FromQuery] int days = 3)
    {
        try
        {
            if (days < 1 || days > 30)
            {
                return BadRequest(new ApiResponse<ForecastSummary>
                {
                    Success = false,
                    Message = "Days parameter must be between 1 and 30"
                });
            }

            var startDate = string.IsNullOrEmpty(fromDate)
                ? DateTime.UtcNow.Date.AddDays(1)
                : DateTime.Parse(fromDate);

            _logger.LogInformation("Generating {Days}-day forecast from {Date}", days, startDate);
            var result = await _forecastingService.ForecastEnergyConsumptionAsync(startDate, days);

            return Ok(new ApiResponse<ForecastSummary>
            {
                Success = true,
                Data = result,
                Message = $"Forecast generated for {result.Forecasts.Count} days"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating forecast");
            return StatusCode(500, new ApiResponse<ForecastSummary>
            {
                Success = false,
                Message = "Error generating forecast",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get simple forecast predictions only (without full summary)
    /// </summary>
    /// <param name="fromDate">Start date for forecast (yyyy-MM-dd)</param>
    /// <param name="days">Number of days to forecast (default: 3)</param>
    [HttpGet("predictions")]
    public async Task<ActionResult<ApiResponse<List<ForecastResult>>>> GetPredictions(
        [FromQuery] string? fromDate = null,
        [FromQuery] int days = 3)
    {
        try
        {
            if (days < 1 || days > 30)
            {
                return BadRequest(new ApiResponse<List<ForecastResult>>
                {
                    Success = false,
                    Message = "Days parameter must be between 1 and 30"
                });
            }

            var startDate = string.IsNullOrEmpty(fromDate)
                ? DateTime.UtcNow.Date.AddDays(1)
                : DateTime.Parse(fromDate);

            var summary = await _forecastingService.ForecastEnergyConsumptionAsync(startDate, days);

            return Ok(new ApiResponse<List<ForecastResult>>
            {
                Success = true,
                Data = summary.Forecasts,
                Message = $"Predictions for {summary.Forecasts.Count} days"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating predictions");
            return StatusCode(500, new ApiResponse<List<ForecastResult>>
            {
                Success = false,
                Message = "Error generating predictions",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}
