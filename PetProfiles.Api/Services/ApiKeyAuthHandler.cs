using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using PetProfiles.Api.Models;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace PetProfiles.Api.Services;

public class ApiKeyAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ApiKeyAuthOptions _apiKeyOptions;

    public ApiKeyAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IOptions<ApiKeyAuthOptions> apiKeyOptions)
        : base(options, logger, encoder, clock)
    {
        _apiKeyOptions = apiKeyOptions.Value;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey(_apiKeyOptions.HeaderName))
        {
            return Task.FromResult(AuthenticateResult.Fail("API Key header not found"));
        }

        var apiKey = Request.Headers[_apiKeyOptions.HeaderName].ToString();

        if (string.IsNullOrEmpty(apiKey) || apiKey != _apiKeyOptions.ApiKey)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "API User"),
            new Claim(ClaimTypes.Role, "ApiUser")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
} 