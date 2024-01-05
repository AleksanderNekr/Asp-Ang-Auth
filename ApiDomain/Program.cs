using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string authScheme = "def";
builder.Services
    .AddAuthentication(authScheme)
    .AddCookie(authScheme);

builder.Services.AddAuthorization();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

RouteGroupBuilder api = app.MapGroup("/api");
api.MapGet("/test", () => "secret")
    .RequireAuthorization()
    .WithOpenApi();

api.MapPost("/login", async context =>
{
    await context.SignInAsync(authScheme, new ClaimsPrincipal(
            new ClaimsIdentity(
                new Claim[]
                {
                    new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
                })),
        new AuthenticationProperties
        {
            IsPersistent = true
        });
}).WithOpenApi();

app.Run();
