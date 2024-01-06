using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string? authScheme = "cookie";
const string? authScheme2 = "cookie2";
const string localAuthScheme = "local";
const string visitorAuthScheme = "visitor";
const string patreonAuthScheme = "patreon-cookie";
builder.Services.AddAuthentication(authScheme)
    .AddCookie(authScheme)
    .AddCookie(authScheme2)
    .AddCookie(localAuthScheme)
    .AddCookie(patreonAuthScheme)
    .AddScheme<CookieAuthenticationOptions, VisitorAuthHandler>(visitorAuthScheme, _ => {})
    .AddOAuth("external-patreon", o =>
    {
        o.SignInScheme = patreonAuthScheme;

        o.ClientId = "id";
        o.ClientSecret = "secret";

        o.AuthorizationEndpoint = "https://oauth.wiremockapi.cloud/oauth/authorize";
        o.TokenEndpoint = "https://oauth.wiremockapi.cloud/oauth/token";
        o.UserInformationEndpoint = "https://oauth.wiremockapi.cloud/userinfo";

        o.CallbackPath = "/cb-patreon";

        o.Scope.Add("profile");
        o.SaveTokens = true;
    });

const string euPolicyName = "eu passport";
const string customerPolicyName = "customer";
builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy(euPolicyName, policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser()
            .AddAuthenticationSchemes(authScheme)
            .RequireClaim("passport_type", "eur");
    })
    .AddPolicy(customerPolicyName, policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser()
            .AddAuthenticationSchemes(localAuthScheme, visitorAuthScheme, patreonAuthScheme);
    })
    .AddPolicy("user", policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser()
            .AddAuthenticationSchemes(localAuthScheme);
    });

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

const string userClaimType = "usr";
app.MapGet("/unsecure", (HttpContext httpContext) => httpContext.User.FindFirstValue(userClaimType) ?? "empty");

app.MapGet("/denmark", (HttpContext httpContext) =>
{
    return "allowed";
});

app.MapGet("/norway", (HttpContext httpContext) =>
{
    return "allowed";
}).RequireAuthorization(euPolicyName);

app.MapGet("/login", async (HttpContext httpContext) =>
{
    List<Claim> claims =
    [
        new Claim(userClaimType, "alex"),
        new Claim("passport_type", "eur")
    ];
    ClaimsIdentity identity = new(claims, authScheme);
    var user = new ClaimsPrincipal(identity);

    await httpContext.SignInAsync(authScheme, user);
    return "ok";
}).AllowAnonymous();

app.MapGet("/", context => Task.FromResult("Hello World!"))
    .RequireAuthorization(customerPolicyName);

app.MapGet("/login-local", async context =>
{
    List<Claim> claims = [new Claim(userClaimType, "Alex")];
    ClaimsIdentity identity = new(claims, localAuthScheme);
    ClaimsPrincipal user = new(identity);

    await context.SignInAsync(localAuthScheme, user);
});

app.MapGet("/login-patreon", async context =>
    await context.ChallengeAsync("external-patreon", new AuthenticationProperties
    {
        RedirectUri = "/"
    })).RequireAuthorization("user");

app.Run();

public class VisitorAuthHandler(
    IOptionsMonitor<CookieAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : CookieAuthenticationHandler(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        AuthenticateResult result = await base.HandleAuthenticateAsync();
        if (result.Succeeded)
        {
            return result;
        }

        List<Claim> claims = [new Claim("usr", "Alex")];
        ClaimsIdentity identity = new(claims, "visitor");
        ClaimsPrincipal user = new(identity);

        await Context.SignInAsync("visitor", user);

        return AuthenticateResult.Success(new AuthenticationTicket(user, "visitor"));
    }
}
