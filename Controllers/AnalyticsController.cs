using HydraEnergyAPI.Models.DTOs;
using HydraEnergyAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HydraEnergyAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(
        IAnalyticsService analyticsService,
        ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive analytics including moving averages and anomaly detection
    /// </summary>
    /// <param name="fromDate">Start date (yyyy-MM-dd)</param>
    /// <param name="toDate">End date (yyyy-MM-dd)</param>
    [HttpGet("summary")]
    public async Task<ActionResult<ApiResponse<AnalyticsSummary>>> GetAnalyticsSummary(
        [FromQuery] string fromDate,
        [FromQuery] string toDate)
    {
        try
        {
            if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate))
            {
                return BadRequest(new ApiResponse<AnalyticsSummary>
                {
                    Success = false,
                    Message = "fromDate and toDate parameters are required",
                    Errors = new List<string> { "Missing required parameters" }
                });
            }

            var from = DateTime.Parse(fromDate);
            var to = DateTime.Parse(toDate);

            _logger.LogInformation("Generating analytics summary from {From} to {To}", fromDate, toDate);
            var result = await _analyticsService.AnalyzeEnergyDataAsync(from, to);

            return Ok(new ApiResponse<AnalyticsSummary>
            {
                Success = true,
                Data = result,
                Message = $"Analytics generated for {result.DailyResults.Count} days with {result.NumberOfAnomalies} anomalies detected"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating analytics summary");
            return StatusCode(500, new ApiResponse<AnalyticsSummary>
            {
                Success = false,
                Message = "Error generating analytics summary",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get anomalies detected in the data
    /// </summary>
    /// <param name="fromDate">Start date (yyyy-MM-dd)</param>
    /// <param name="toDate">End date (yyyy-MM-dd)</param>
    [HttpGet("anomalies")]
    public async Task<ActionResult<ApiResponse<List<AnalyticsResult>>>> GetAnomalies(
        [FromQuery] string fromDate,
        [FromQuery] string toDate)
    {
        try
        {
            if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate))
            {
                return BadRequest(new ApiResponse<List<AnalyticsResult>>
                {
                    Success = false,
                    Message = "fromDate and toDate parameters are required"
                });
            }

            var from = DateTime.Parse(fromDate);
            var to = DateTime.Parse(toDate);

            var summary = await _analyticsService.AnalyzeEnergyDataAsync(from, to);
            var anomalies = summary.DailyResults.Where(r => r.IsAnomaly).ToList();

            return Ok(new ApiResponse<List<AnalyticsResult>>
            {
                Success = true,
                Data = anomalies,
                Message = $"Found {anomalies.Count} anomalies"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching anomalies");
            return StatusCode(500, new ApiResponse<List<AnalyticsResult>>
            {
                Success = false,
                Message = "Error fetching anomalies",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}
