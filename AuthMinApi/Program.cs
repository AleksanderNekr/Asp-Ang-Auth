using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string? authScheme = "cookie";
builder.Services.AddAuthentication(authScheme)
    .AddCookie(authScheme);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

const string userClaimType = "usr";
app.MapGet("/unsecure", (HttpContext httpContext) => httpContext.User.FindFirstValue(userClaimType) ?? "empty");

app.MapGet("/sweden", (HttpContext httpContext) =>
{
    if (httpContext.User.Identities.All(id => id.AuthenticationType != authScheme))
    {
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return string.Empty;
    }

    if (!httpContext.User.HasClaim("passport_type", "eur"))
    {
        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        return string.Empty;
    }

    return "allowed";
});

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
});

app.Run();
