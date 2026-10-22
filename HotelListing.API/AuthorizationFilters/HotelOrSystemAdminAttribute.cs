using HotelListing.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;

namespace HotelListing.API.AuthorizationFilters;


public class HotelOrSystemAdminAttribute(Type type) : TypeFilterAttribute(type)
{

}


public class HotelOrSystemAdminFilter(HotelListingDbContext dbContext) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpUser = context.HttpContext.User;
        if (httpUser?.Identity?.IsAuthenticated == false)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (httpUser!.IsInRole("Administrator"))
        {
            return;
        }

        var userId = httpUser.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
        {
            context.Result = new ForbidResult();
            return;
        }

        // try to get the hotelId from route values
        context.RouteData.Values.TryGetValue("hotelId", out var hotelId);
        int.TryParse(hotelId?.ToString(), out int parsedHotelId);
        if (parsedHotelId == 0)
        {
            context.Result = new ForbidResult();
            return;
        }

        // Check if user is an admin for this specific hotel
        var isHotelAdminUser = await dbContext.HotelAdmins.AnyAsync(q => q.UserId == userId && q.Hotelid == hotelId);
    }
}


