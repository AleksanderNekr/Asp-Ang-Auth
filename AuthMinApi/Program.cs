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

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.Use((context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/login"))
    {
        return next();
    }
    
    if (context.User.Identities.All(id => id.AuthenticationType != authScheme))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    }
    
    if (!context.User.HasClaim("passport_type", "eur"))
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }

    return next();
});

const string userClaimType = "usr";
app.MapGet("/unsecure", (HttpContext httpContext) => httpContext.User.FindFirstValue(userClaimType) ?? "empty");

// [AuthScheme(authScheme2)]
app.MapGet("/denmark", (HttpContext httpContext) =>
{
    // if (httpContext.User.Identities.All(id => id.AuthenticationType != authScheme2))
    // {
    //     httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
    //     return string.Empty;
    // }
    //
    // if (!httpContext.User.HasClaim("passport_type", "eur"))
    // {
    //     httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
    //     return string.Empty;
    // }

    return "allowed";
});

app.MapGet("/norway", (HttpContext httpContext) =>
{
    // if (httpContext.User.Identities.All(id => id.AuthenticationType != authScheme))
    // {
    //     httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
    //     return string.Empty;
    // }
    //
    // if (!httpContext.User.HasClaim("passport_type", "NOR"))
    // {
    //     httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
    //     return string.Empty;
    // }

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
