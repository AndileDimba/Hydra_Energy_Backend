using HydraEnergyAPI.Models.DTOs;
using HydraEnergyAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HydraEnergyAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnergyController : ControllerBase
{
    private readonly IEnergyDataService _energyDataService;
    private readonly ILogger<EnergyController> _logger;

    public EnergyController(
        IEnergyDataService energyDataService,
        ILogger<EnergyController> logger)
    {
        _energyDataService = energyDataService;
        _logger = logger;
    }

    /// <summary>
    /// Get energy consumption data for a date range
    /// </summary>
    /// <param name="fromDate">Start date (yyyy-MM-dd)</param>
    /// <param name="toDate">End date (yyyy-MM-dd)</param>
    [HttpGet("data")]
    public async Task<ActionResult<ApiResponse<List<EnergyDataResponse>>>> GetEnergyData(
        [FromQuery] string fromDate,
        [FromQuery] string toDate)
    {
        try
        {
            if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate))
            {
                return BadRequest(new ApiResponse<List<EnergyDataResponse>>
                {
                    Success = false,
                    Message = "fromDate and toDate parameters are required",
                    Errors = new List<string> { "Missing required parameters" }
                });
            }

            _logger.LogInformation("Fetching energy data from {From} to {To}", fromDate, toDate);
            var result = await _energyDataService.GetEnergyDataAsync(fromDate, toDate);

            return Ok(new ApiResponse<List<EnergyDataResponse>>
            {
                Success = true,
                Data = result,
                Message = $"Retrieved {result.Count} records"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching energy data");
            return StatusCode(500, new ApiResponse<List<EnergyDataResponse>>
            {
                Success = false,
                Message = "Error fetching energy data",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get total energy consumption for a date range
    /// </summary>
    [HttpGet("total")]
    public async Task<ActionResult<ApiResponse<double>>> GetTotalConsumption(
        [FromQuery] string fromDate,
        [FromQuery] string toDate)
    {
        try
        {
            if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate))
            {
                return BadRequest(new ApiResponse<double>
                {
                    Success = false,
                    Message = "fromDate and toDate parameters are required"
                });
            }

            var from = DateTime.Parse(fromDate);
            var to = DateTime.Parse(toDate);
            var total = await _energyDataService.GetTotalConsumptionAsync(from, to);

            return Ok(new ApiResponse<double>
            {
                Success = true,
                Data = total,
                Message = $"Total consumption: {total:F2} kWh"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total consumption");
            return StatusCode(500, new ApiResponse<double>
            {
                Success = false,
                Message = "Error calculating total consumption",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get average daily energy consumption for a date range
    /// </summary>
    [HttpGet("average")]
    public async Task<ActionResult<ApiResponse<double>>> GetAverageConsumption(
        [FromQuery] string fromDate,
        [FromQuery] string toDate)
    {
        try
        {
            if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate))
            {
                return BadRequest(new ApiResponse<double>
                {
                    Success = false,
                    Message = "fromDate and toDate parameters are required"
                });
            }

            var from = DateTime.Parse(fromDate);
            var to = DateTime.Parse(toDate);
            var average = await _energyDataService.GetAverageDailyConsumptionAsync(from, to);

            return Ok(new ApiResponse<double>
            {
                Success = true,
                Data = average,
                Message = $"Average daily consumption: {average:F2} kWh"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating average consumption");
            return StatusCode(500, new ApiResponse<double>
            {
                Success = false,
                Message = "Error calculating average consumption",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}
