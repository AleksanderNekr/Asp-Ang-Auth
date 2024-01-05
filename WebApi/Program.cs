using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

const string authScheme = "def";
builder.Services
    .AddAuthentication(authScheme)
    .AddCookie(authScheme);

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("front", policyBuilder => policyBuilder
        .WithOrigins("https://localhost:4200")
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("front");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

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
