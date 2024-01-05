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

const string claimType = "usr";
app.MapGet("/username", (HttpContext httpContext) =>
{
    return httpContext.User.FindFirstValue(claimType);
});

app.MapGet("/login", async (HttpContext httpContext) =>
{
    List<Claim> claims = [new Claim(claimType, "alex")];
    ClaimsIdentity identity = new(claims, authScheme);
    var user = new ClaimsPrincipal(identity);

    await httpContext.SignInAsync(authScheme, user);
    return "ok";
});

app.Run();
