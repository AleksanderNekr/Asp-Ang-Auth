using AuthSchemas;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.Json;
using System.Security.Claims;
using System.Text.Json.Serialization;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication()
    .AddCookie("local")
    .AddScheme<CookieAuthenticationOptions, VisitorAuthHandler>("visitor", _ => {});
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("customer", policyBuilder =>
    {
        policyBuilder.AddAuthenticationSchemes("local", "visitor")
            .RequireAuthenticatedUser();
    });

builder.Services.Configure<JsonOptions>(options => options.SerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);

WebApplication app = builder.Build();

app.UseAuthentication()
    .UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", GetDefault)
    .Produces<string>()
    .RequireAuthorization("customer");

app.MapGet("/login-local", LoginLocal)
    .Produces(200)
    .Produces(400);

app.UseHttpsRedirection();

app.Run();

return;

IResult GetDefault()
{
    return Results.Ok("Hello World!");
}

IResult LoginLocal(HttpContext context)
{
    List<Claim> claims = [ new Claim("usr", "Alex") ];
    ClaimsIdentity identity = new(claims, "local");
    ClaimsPrincipal user = new(identity);
    context.SignInAsync("local", user);

    return user.Identity?.IsAuthenticated ?? false
        ? Results.Ok()
        : Results.BadRequest();
}
