namespace HotelListing.API.Common.Models;

public sealed class JwtSettings
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Key { get; init; } = string.Empty;
    public string DurationInMinutes { get; init; } = string.Empty;
}
