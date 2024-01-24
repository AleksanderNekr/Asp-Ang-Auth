using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace AuthSchemas;

public class LichessOAuthOptions : OAuthOptions
{
    public LichessOAuthOptions()
    {
        AuthorizationEndpoint   = "https://lichess.org/oauth/authorize";
        TokenEndpoint           = "https://lichess.org/api/token";
        UserInformationEndpoint = "https://lichess.org/api/account";
        UserEmailsEndpoint      = "https://lichess.org/api/account/email";

        ClientId     = "test-oauth-app";
        ClientSecret = "123qweasdrty";

        SaveTokens = true;

        Scope.Add("email:read");
        Scope.Add("follow:read");

        CallbackPath = "/oauth-cb-lichess";
        SignInScheme = "lichess-cookie";

        UsePkce = true;

        ClaimsIssuer = "lichess-oauth";
        ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
        ClaimActions.MapJsonKey(ClaimTypes.Name, "username");
        ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        ClaimActions.MapJsonSubKey(ClaimTypes.GivenName, "profile", "firstName");
        ClaimActions.MapJsonSubKey(ClaimTypes.Surname, "profile", "lastName");
    }

    public readonly string UserEmailsEndpoint;
}
