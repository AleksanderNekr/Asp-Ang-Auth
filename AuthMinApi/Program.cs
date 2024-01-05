using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDataProtection();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Use((context, next) =>
{
    string? authCookie = context.Request.Headers.Cookie.FirstOrDefault(s => s is not null && s.StartsWith("auth="));
    if (authCookie is null)
    {

        return next();
    }

    var protectionProvider = context.RequestServices.GetRequiredService<IDataProtectionProvider>();
    string payload = authCookie.Split("=").Last();
    payload = protectionProvider
        .CreateProtector("auth-cookie")
        .Unprotect(payload);

    string[] parts = payload.Split(":");
    string key = parts[0];
    string value = parts[1];

    List<Claim> claims = [new Claim(key, value)];
    ClaimsIdentity identity = new(claims);
    context.User = new ClaimsPrincipal(identity);

    return next();
});

app.MapGet("/username", (HttpContext httpContext) =>
{
    return httpContext.User.FindFirstValue("usr");
});

app.MapGet("/login", (AuthService authService) =>
{
    authService.SignIn();
    return "ok";
});

app.Run();

internal class AuthService
{
    private readonly IDataProtectionProvider _protectionProvider;
    private readonly IHttpContextAccessor _contextAccessor;
    public AuthService(IDataProtectionProvider protectionProvider, IHttpContextAccessor contextAccessor)
    {
        _protectionProvider = protectionProvider;
        _contextAccessor = contextAccessor;
    }

    public void SignIn()
    {
        IDataProtector protector = _protectionProvider.CreateProtector("auth-cookie");
        if (_contextAccessor.HttpContext is not null)
        {
            _contextAccessor.HttpContext.Response.Headers.SetCookie = $"auth={protector.Protect("usr:alex")}";
        }


    }
}
