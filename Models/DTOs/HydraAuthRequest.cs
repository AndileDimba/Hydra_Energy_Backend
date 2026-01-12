namespace HydraEnergyAPI.Models.DTOs;

public class HydraAuthRequest
{
    public string ClientId { get; set; } = "ro.client";
    public string GrantType { get; set; } = "password";
    public string ClientSecret { get; set; } = "secret";
    public string Scope { get; set; } = "api1";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
