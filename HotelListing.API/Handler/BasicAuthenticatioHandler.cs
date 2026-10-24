using HotelListing.API.Application.Contracts;
using HotelListing.API.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace HotelListing.API.Handler;

public class BasicAuthenticatioHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IUsersService usersService
    ) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeaderValues))
        {
            return AuthenticateResult.NoResult();
        }

        var authHeader = authHeaderValues.ToString();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var token = authHeader["Basic ".Length..].Trim();
        string decodedToken;

        try
        {
            var credntialBytes = Convert.FromBase64String(token);
            decodedToken = Encoding.UTF8.GetString(credntialBytes);
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Authorization Header");
        }

        var separatorIndex = decodedToken.IndexOf(':');
        if (separatorIndex == -1)
        {
            return AuthenticateResult.Fail("Invalid Authorization Header");
        }

        var userNameOrEmail = decodedToken[..separatorIndex];
        var password = decodedToken[(separatorIndex + 1)..];
        var loginDto = new LoginDto
        {
            Email = userNameOrEmail,
            Password = password
        };

        var result = await usersService.LoginAsync(loginDto);
        if (!result.IsSuccess)
        {
            return AuthenticateResult.Fail("Invalid credentials");
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userNameOrEmail)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);

    }
}
