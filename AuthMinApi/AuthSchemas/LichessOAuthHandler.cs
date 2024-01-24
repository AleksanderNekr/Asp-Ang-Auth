using System.Net.Http.Headers;
using System.Net.Mime;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;

// ReSharper disable HollowTypeName

namespace AuthSchemas;

public class LichessOAuthHandler(
    IOptionsMonitor<LichessOAuthOptions> options,
    ILoggerFactory                       loggerFactory,
    UrlEncoder                           encoder)
    : OAuthHandler<LichessOAuthOptions>(options, loggerFactory, encoder)
{
    /// <inheritdoc />
    protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity           identity,
                                                                          AuthenticationProperties properties,
                                                                          OAuthTokenResponse       tokens)
    {
        ILogger<LichessOAuthHandler> logger = loggerFactory.CreateLogger<LichessOAuthHandler>();
        logger.LogInformation("Starting creating ticket");

        using JsonDocument payload = await RequestUserInformationAsync(Options.UserInformationEndpoint, tokens.AccessToken!)
                                  ?? throw new HttpRequestException("Error while trying to get USER PROFILE");
        logger.LogInformation("Got payload\n{Payload}", payload.RootElement.ToString());

        ClaimsPrincipal            user          = new(identity);
        OAuthCreatingTicketContext ticketContext = new(user, properties, Context, Scheme, Options, Backchannel, tokens, payload.RootElement);
        ticketContext.RunClaimActions();
        logger.LogInformation("Ran claim actions");

        if (EmailCanBeRetrieved())
        {
            using JsonDocument emailPayload = await RequestUserInformationAsync(Options.UserEmailsEndpoint, tokens.AccessToken!)
                                           ?? throw new HttpRequestException("Error while trying to get EMAIL");
            logger.LogInformation("Got email payload\n{Payload}", emailPayload.RootElement.ToString());
            ticketContext.RunClaimActions(emailPayload.RootElement);
        }

        await Events.CreatingTicket(ticketContext);
        AuthenticationTicket ticket = new(ticketContext.Principal!, ticketContext.Properties, Scheme.Name);
        logger.LogInformation("Ticket created\n{@Ticket}", ticket);

        return ticket;

        bool EmailCanBeRetrieved()
        {
            return !string.IsNullOrEmpty(Options.UserEmailsEndpoint)
                && !identity.HasClaim(claim => claim.Type == ClaimTypes.Email)
                && Options.Scope.Contains("email:read");
        }
    }

    /// <inheritdoc />
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        ILogger<LichessOAuthHandler> logger = loggerFactory.CreateLogger<LichessOAuthHandler>();
        logger.LogInformation("Starting authenticating");

        AuthenticateResult result = await Context.AuthenticateAsync(SignInScheme);
        logger.LogInformation("Authentication result: {Result}",
                              result.Succeeded
                                  ? "success"
                                  : "fail");

        if (result.Failure != null)
        {
            return result;
        }

        // The SignInScheme may be shared with multiple providers, make sure this provider issued the identity.
        AuthenticationTicket? ticket = result.Ticket;

        if (ticket is null
         || !ticket.Properties.Items.TryGetValue(".AuthScheme", out var authenticatedScheme)
         || !string.Equals(Scheme.Name, authenticatedScheme, StringComparison.Ordinal))
        {
            return AuthenticateResult.NoResult();
        }

        logger.LogInformation("Auth succeeded");

        return AuthenticateResult.Success(new AuthenticationTicket(ticket.Principal,
                                                                   ticket.Properties,
                                                                   Scheme.Name));
    }

    private async Task<JsonDocument?> RequestUserInformationAsync(string userInfoEndpoint,
                                                                  string accessToken)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, userInfoEndpoint);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        using HttpResponseMessage response = await Backchannel.SendAsync(
                                                 request,
                                                 HttpCompletionOption.ResponseHeadersRead,
                                                 Context.RequestAborted);

        return response.IsSuccessStatusCode
                   ? JsonDocument.Parse(await response.Content.ReadAsStringAsync(Context.RequestAborted))
                   : null;
    }
}
