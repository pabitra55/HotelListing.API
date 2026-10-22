using HotelListing.Api.Results;
using HotelListing.API.Contracts;
using HotelListing.API.Data;
using HotelListing.API.DTOs.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListing.API.Services;

public class UsersService(UserManager<ApplicationUser> userManager, IConfiguration configuration, HttpContextAccessor httpContextAccessor) : IUsersService
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
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"] ?? string.Empty));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Create an encoded token
        var token = new JwtSecurityToken(
            issuer: configuration["JwtSettings:Issuer"],
            audience: configuration["JwtSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(configuration["JwtSettings:DurationInMinutes"])),
            signingCredentials: credentials
        );

        // Return token value
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string UserId => httpContextAccessor?
                      .HttpContext?
                      .User
                      .FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? string.Empty;
}
