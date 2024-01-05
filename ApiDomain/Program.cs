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

app.UseRouting();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(_ => {});

app.UseSpa(spaBuilder => spaBuilder.UseProxyToSpaDevelopmentServer("http://localhost:4200"));

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
                },
                authScheme)),
        new AuthenticationProperties
        {
            IsPersistent = true
        });
}).WithOpenApi();

app.Run();
