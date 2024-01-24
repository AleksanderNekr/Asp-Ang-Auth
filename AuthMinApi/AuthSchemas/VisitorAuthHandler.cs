namespace AuthSchemas;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
public class VisitorAuthHandler(
    IOptionsMonitor<CookieAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : CookieAuthenticationHandler(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        AuthenticateResult baseResult = await base.HandleAuthenticateAsync();
        if (baseResult.Succeeded)
        {
            return baseResult;
        }

        List<Claim> claims = [ new Claim("usr", "Alex") ];
        ClaimsIdentity identity = new(claims, "visitor");
        ClaimsPrincipal user = new(identity);

        await Context.SignInAsync("visitor", user);

        return AuthenticateResult.Success(new AuthenticationTicket(user, "visitor"));
    }
}
