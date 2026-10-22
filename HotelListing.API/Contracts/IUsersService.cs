using HotelListing.Api.Results;
using HotelListing.API.DTOs.Auth;

namespace HotelListing.API.Contracts
{
    public interface IUsersService
    {
        string UserId { get; }

        Task<Result<string>> LoginAsync(LoginDto loginDto);
        Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto);
    }
}