using HydraEnergyAPI.Models.DTOs;

namespace HydraEnergyAPI.Services;

public interface IHydraAuthService
{
    Task<string> GetAccessTokenAsync();
    Task<HydraAuthResponse> AuthenticateAsync();
    Task<bool> ValidateTokenAsync();
}
