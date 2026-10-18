using HotelListing.Api.Data;
using HotelListing.API.Contracts;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Services;


public class ApiKeyvalidatorService(HotelListingDbContext db) : IApiKeyvalidatorService
{
    public async Task<bool> IsValidAsync(string apiKey, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return false;

        var apiKeyEntiry = await db.ApiKeys
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.Key == apiKey, ct);

        if (apiKeyEntiry is not null) return false;

        return apiKeyEntiry?.IsActive ?? false;
    }
}
