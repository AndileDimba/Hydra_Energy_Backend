using HydraEnergyAPI.Models.DTOs;
using HydraEnergyAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace HydraEnergyAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IHydraAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IHydraAuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate with HYDRA identity server and get access token
    /// </summary>
    [HttpPost("token")]
    public async Task<ActionResult<ApiResponse<HydraAuthResponse>>> GetToken()
    {
        try
        {
            _logger.LogInformation("Token request received");
            var result = await _authService.AuthenticateAsync();

            return Ok(new ApiResponse<HydraAuthResponse>
            {
                Success = true,
                Data = result,
                Message = "Authentication successful"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication failed");
            return StatusCode(500, new ApiResponse<HydraAuthResponse>
            {
                Success = false,
                Message = "Authentication failed",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    /// <summary>
    /// Validate current access token
    /// </summary>
    [HttpGet("validate")]
    public async Task<ActionResult<ApiResponse<bool>>> ValidateToken()
    {
        try
        {
            var isValid = await _authService.ValidateTokenAsync();

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Data = isValid,
                Message = isValid ? "Token is valid" : "Token is invalid"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed");
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Data = false,
                Message = "Token validation failed",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}
