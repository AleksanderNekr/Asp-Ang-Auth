using AuthSchemas;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text.Json.Serialization;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.AddAuthentication()
    .AddCookie("local")
    .AddScheme<CookieAuthenticationOptions, VisitorAuthHandler>("visitor", _ => {})
    .AddCookie("patreon-cookie")
    .AddOAuth("external-patreon", options =>
    {
        options.AuthorizationEndpoint = "https://oauth.wiremockapi.cloud/oauth/authorize";
        options.TokenEndpoint = "https://oauth.wiremockapi.cloud/oauth/token";
        options.UserInformationEndpoint = "https://oauth.wiremockapi.cloud/userinfo";

        options.ClientId = "test-oauth-app";
        options.ClientSecret = "123qweasdrty";

        options.SaveTokens = true;

        options.Scope.Add("profile");

        options.CallbackPath = "/oauth-cb-patreon";
        options.SignInScheme = "patreon-cookie";
    }).AddCookie("lichess-cookie")
       .AddOAuth<LichessOAuthOptions, LichessOAuthHandler>("lichess-oauth", _ => {});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("customer", policyBuilder =>
    {
        policyBuilder.AddAuthenticationSchemes("patreon-cookie", "local", "visitor")
            .RequireAuthenticatedUser();
    })
    .AddPolicy("user", policyBuilder =>
    {
        policyBuilder.AddAuthenticationSchemes("local")
            .RequireAuthenticatedUser();
    })
    .AddPolicy("lichess-user", policyBuilder =>
    {
      policyBuilder.AddAuthenticationSchemes("lichess-oauth")
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

app.MapGet("/login-patreon", LoginPatreon)
    .Produces<string>()
    .Produces(400)
    .RequireAuthorization("user");

app.MapGet("/login-lichess", LoginLichess);

app.MapGet("/lichess-info", LichessUserInfo)
   .RequireAuthorization("lichess-user");

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

IResult LoginPatreon(HttpContext context)
{
    return Results.Challenge(new AuthenticationProperties { RedirectUri = "/" }, [ "external-patreon" ]);
}

IResult LoginLichess(HttpContext context)
{
    return Results.Challenge(new AuthenticationProperties { RedirectUri = "/" }, [ "lichess-oauth" ]);
}

IResult LichessUserInfo(HttpContext context)
{
    return Results.Ok(context.User.Identities);
}
