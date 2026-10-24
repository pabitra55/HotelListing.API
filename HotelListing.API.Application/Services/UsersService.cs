using HotelListing.Api.Common.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HotelListing.API.Common.Constants;
using HotelListing.API.Common.Models;
using Microsoft.Extensions.Options;
using HotelListing.API.Domain.Data;
using Microsoft.AspNetCore.Http;
using HotelListing.API.Application.DTOs.Auth;
using HotelListing.API.Application.Contracts;


namespace HotelListing.API.Application.Services;

public class UsersService(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtSettings, IHttpContextAccessor httpContextAccessor, HotelListingDbContext hotelListingDbContext) : IUsersService
{
    public async Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto)
    {
        var user = new ApplicationUser
        {
            UserName = registerUserDto.Email,
            Email = registerUserDto.Email,
            FirstName = registerUserDto.FirstName,
            LastName = registerUserDto.LastName
        };
        var result = await userManager.CreateAsync(user, registerUserDto.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new Error(ErrorCodes.BadRequest, e.Description)).ToArray();
            return Result<RegisteredUserDto>.BadRequest(errors);
        }
        await userManager.AddToRoleAsync(user, registerUserDto.Role);

        // If Hotel Afmin, add to HotelAdmins table
        if (registerUserDto.Role == RoleNames.HotelAdmin)
        {
            var hotelAdmin = hotelListingDbContext.HotelAdmins.Add(new HotelAdmin
            {
                UserId = user.Id,
                Hotelid = registerUserDto.AssociatedHotelId.GetValueOrDefault()

            });
            await hotelListingDbContext.SaveChangesAsync();
        }

        var registeredUser = new RegisteredUserDto
        {
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Id = user.Id,
            Role = registerUserDto.Role
        };

        //Optional: Send confirmation email or perform additional actions after successful registration


        return Result<RegisteredUserDto>.Success(registeredUser);
    }

    public async Task<Result<string>> LoginAsync(LoginDto loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);
        if (user == null)
        {
            return Result<string>.Failure(new Error(ErrorCodes.BadRequest, "Invalid credentials"));
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
        {
            return Result<string>.Failure(new Error(ErrorCodes.BadRequest, "Invalid credentials"));
        }
        var token = await GenerateJwtToken(user);

        return Result<string>.Success(token);
    }

    /// <summary>
    /// GenerateJwtToken
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        // Set basic user claims
        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Sub, user.Id),
            new (JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Name, user.FullName)
        };

        // Set user role claims
        var roles = await userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();

        claims = claims.Union(roleClaims).ToList();

        // Set JWT key credentials
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Value.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Create an encoded token
        var token = new JwtSecurityToken(
            issuer: jwtSettings.Value.Issuer,
            audience: jwtSettings.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(jwtSettings.Value.DurationInMinutes)),
            signingCredentials: credentials
        );

        // Return token value
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string UserId => httpContextAccessor?
                      .HttpContext?
                      .User
                      .FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? httpContextAccessor?
                      .HttpContext?
                      .User
                      .FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? string.Empty;
}
