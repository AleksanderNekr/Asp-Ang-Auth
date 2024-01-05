using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

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
        .WithOrigins("https://localhost:4200", "https://172.21.16.1:4200", "https://192.168.0.3:4200")
        .AllowCredentials()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

WebApplication app = builder.Build();

app.UseCors("front");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.Use((context, next) =>
{
    if (context.User.Identity.IsAuthenticated
        && !context.Request.Headers.Cookie.Any(x => x.Contains("user-info", StringComparison.Ordinal)))
    {
        var user = new {UserName = "Alex"};
        string userJson = JsonSerializer.Serialize(user);
        string userBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(userJson));
        context.Response.Cookies.Append("user-info-payload", userBase64);
        context.Response.Cookies.Append("user-info", "1");
    }
    return next();
});

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
