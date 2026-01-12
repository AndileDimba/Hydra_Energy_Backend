using HydraEnergyAPI.Models.DTOs;
using HydraEnergyAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HydraEnergyAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InsightsController : ControllerBase
{
    private readonly IInsightsService _insightsService;
    private readonly ILogger<InsightsController> _logger;

    public InsightsController(
        IInsightsService insightsService,
        ILogger<InsightsController> logger)
    {
        _insightsService = insightsService;
        _logger = logger;
    }

    /// <summary>
    /// Generate comprehensive insights combining energy, weather, and analytics data
    /// </summary>
    /// <param name="fromDate">Start date (yyyy-MM-dd)</param>
    /// <param name="toDate">End date (yyyy-MM-dd)</param>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<InsightsSummary>>> GetInsights(
        [FromQuery] string fromDate,
        [FromQuery] string toDate)
    {
        try
        {
            if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate))
            {
                return BadRequest(new ApiResponse<InsightsSummary>
                {
                    Success = false,
                    Message = "fromDate and toDate parameters are required",
                    Errors = new List<string> { "Missing required parameters" }
                });
            }

            var from = DateTime.Parse(fromDate);
            var to = DateTime.Parse(toDate);

            _logger.LogInformation("Generating insights from {From} to {To}", fromDate, toDate);
            var result = await _insightsService.GenerateInsightsAsync(from, to);

            return Ok(new ApiResponse<InsightsSummary>
            {
                Success = true,
                Data = result,
                Message = $"Generated {result.Insights.Count} insights"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating insights");
            return StatusCode(500, new ApiResponse<InsightsSummary>
            {
                Success = false,
                Message = "Error generating insights",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get insights filtered by type
    /// </summary>
    /// <param name="fromDate">Start date (yyyy-MM-dd)</param>
    /// <param name="toDate">End date (yyyy-MM-dd)</param>
    /// <param name="type">Insight type filter</param>
    [HttpGet("type/{type}")]
    public async Task<ActionResult<ApiResponse<List<InsightResult>>>> GetInsightsByType(
        [FromQuery] string fromDate,
        [FromQuery] string toDate,
        string type)
    {
        try
        {
            if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate))
            {
                return BadRequest(new ApiResponse<List<InsightResult>>
                {
                    Success = false,
                    Message = "fromDate and toDate parameters are required"
                });
            }

            var from = DateTime.Parse(fromDate);
            var to = DateTime.Parse(toDate);

            var summary = await _insightsService.GenerateInsightsAsync(from, to);
            var filteredInsights = summary.Insights
                .Where(i => i.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(new ApiResponse<List<InsightResult>>
            {
                Success = true,
                Data = filteredInsights,
                Message = $"Found {filteredInsights.Count} insights of type '{type}'"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching insights by type");
            return StatusCode(500, new ApiResponse<List<InsightResult>>
            {
                Success = false,
                Message = "Error fetching insights",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get insights filtered by severity
    /// </summary>
    /// <param name="fromDate">Start date (yyyy-MM-dd)</param>
    /// <param name="toDate">End date (yyyy-MM-dd)</param>
    /// <param name="severity">Severity filter (info, warning, critical)</param>
    [HttpGet("severity/{severity}")]
    public async Task<ActionResult<ApiResponse<List<InsightResult>>>> GetInsightsBySeverity(
        [FromQuery] string fromDate,
        [FromQuery] string toDate,
        string severity)
    {
        try
        {
            if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate))
            {
                return BadRequest(new ApiResponse<List<InsightResult>>
                {
                    Success = false,
                    Message = "fromDate and toDate parameters are required"
                });
            }

            var from = DateTime.Parse(fromDate);
            var to = DateTime.Parse(toDate);

            var summary = await _insightsService.GenerateInsightsAsync(from, to);
            var filteredInsights = summary.Insights
                .Where(i => i.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(new ApiResponse<List<InsightResult>>
            {
                Success = true,
                Data = filteredInsights,
                Message = $"Found {filteredInsights.Count} insights with severity '{severity}'"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching insights by severity");
            return StatusCode(500, new ApiResponse<List<InsightResult>>
            {
                Success = false,
                Message = "Error fetching insights",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}
