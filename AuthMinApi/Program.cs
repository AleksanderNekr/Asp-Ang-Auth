using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string? authScheme = "cookie";
const string? authScheme2 = "cookie2";
builder.Services.AddAuthentication(authScheme)
    .AddCookie(authScheme)
    .AddCookie(authScheme2);

const string policyName = "eu passport";
builder.Services
    .AddAuthorizationBuilder()
    .AddPolicy(policyName, policyBuilder =>
    {
        policyBuilder.RequireAuthenticatedUser()
            .AddAuthenticationSchemes(authScheme)
            .RequireClaim("passport_type", "eur");
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
}).RequireAuthorization(policyName);

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

app.Run();
