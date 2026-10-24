using HotelListing.Api.Common.Results;
using HotelListing.API.Application.DTOs.Auth;

namespace HotelListing.API.Application.Contracts
{
    public interface IUsersService
    {
        string UserId { get; }

        Task<Result<string>> LoginAsync(LoginDto loginDto);
        Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto);
    }
}