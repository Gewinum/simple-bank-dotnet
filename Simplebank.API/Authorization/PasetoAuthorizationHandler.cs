using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Simplebank.Domain.Interfaces.Providers;

namespace Simplebank.API.Authorization;

public static class AuthenticationExtensions
{
    public static AuthenticationBuilder AddPasetoAuthentication(
        this IServiceCollection services,
        string scheme,
        Action<AuthenticationSchemeOptions> configureOptions)
    {
        return services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = scheme;
                options.DefaultChallengeScheme = scheme;
            })
            .AddScheme<AuthenticationSchemeOptions, PasetoAuthenticationHandler>(scheme, options =>
            {
                configureOptions(options);
                options.TimeProvider = TimeProvider.System;
            });
    }
}

public class PasetoAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ITokensProvider _tokensProvider;

    public PasetoAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ITokensProvider tokensProvider)
        : base(options, logger, encoder)
    {
        _tokensProvider = tokensProvider;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();
        var tokenInfo = _tokensProvider.ValidateToken(token);

        if (tokenInfo == null)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid token"));
        }

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, tokenInfo.UserId.ToString()) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}