using System.Security;

namespace HotelListing.API.Application.Contracts;

public interface IApiKeyvalidatorService
{
    Task<bool> IsValidAsync(string apiKey, CancellationToken ct = default);
}