#r "devoctomy.funk.core.dll"

using devoctomy.funk.core.Azure.Functions;
using devoctomy.funk.core.Cryptography;
using devoctomy.funk.core.Environment;
using devoctomy.funk.core.Extensions;
using devoctomy.funk.core.Membership;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Net;
using System.Web;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    FunctionResponseBase pFRBResponse = new FunctionResponseBase();

    log.Info("Getting current authenticated users email address.");
    String pStrEmail = await GetAuthenticatedUserEmailAddressAsync(req);

    log.Info("Initialising membership storage.");
    Storage pStoMembership = new Storage("TableStorageRootURL", "AzureWebJobsStorage", "UserInfo");

    log.Info($"Getting registered user associated with '{pStrEmail}'.");
    ClaimsPrincipal pCPlUser = CreateUserPrincipal(pStrEmail);
    User pUsrUser = pStoMembership.GetUser(pCPlUser);

    if (pUsrUser != null)
    {
        log.Info("Associated user exists, checking existing activation status.");
        if (!pUsrUser.Activated)
        {
            log.Info("User not activated.");
            return (req.CreateResponse(HttpStatusCode.MethodNotAllowed, "User not activated."));
        }
    }
    else
    {
        log.Info("No associated user was found.");

        return (req.CreateResponse(HttpStatusCode.MethodNotAllowed, "Unknown user."));
    }

    switch (req.Method.Method)
    {
        case "GET":
            {
                log.Info("Getting user profile.");

                Profile pProProfile = pUsrUser.GetProfile(pStoMembership);
                String pStrRetVal = pFRBResponse.ToJSON(true,
                    HttpStatusCode.OK,
                    pProProfile,
                    Newtonsoft.Json.Formatting.None);

                HttpResponseMessage pHRMResponse = new HttpResponseMessage(HttpStatusCode.OK);
                pHRMResponse.Content = new StringContent(pStrRetVal);
                return (pHRMResponse);
            }
        case "PUT":
            {
                log.Info("Updating user profile.");

                String pStrContent = await req.Content.ReadAsStringAsync();
                Profile pProSource = Profile.FromJSON(pStrContent);
                Profile pProTarget = pUsrUser.GetProfile(pStoMembership);
                pProTarget.SetFrom(pProSource);
                pProTarget.Replace(pStoMembership, pUsrUser);

                return (req.CreateResponse(HttpStatusCode.OK, "Profile updated."));
            }
        default:
            {
                return (req.CreateResponse(HttpStatusCode.BadRequest, "Method not supported."));
            }
    }
}

private static ClaimsPrincipal CreateUserPrincipal(String iEmail)
{
    GenericIdentity pGIyTestUser = new GenericIdentity("John Doe");
    pGIyTestUser.AddClaim(new Claim(ClaimTypes.Email, iEmail));
    ClaimsPrincipal pCPlTestUser = new ClaimsPrincipal(pGIyTestUser);
    return (pCPlTestUser);
}

private static async Task<String> GetAuthenticatedUserEmailAddressAsync(HttpRequestMessage req)
{
    if (req.Headers.Contains("x-zumo-auth"))
    {
        String pStrXZumoAuth = req.Headers.GetValues("x-zumo-auth").First();
        String pStrEmail = await FunctionsHelpers.GetAuthProviderClaim(String.Format("https://{0}/{1}", req.RequestUri.Host, ".auth/me"),
            pStrXZumoAuth,
            ClaimTypes.Email);
        return (pStrEmail);
    }
    else
    {
        ClaimsPrincipal pCPlCurrentUser = ClaimsPrincipal.Current;
        if (pCPlCurrentUser != null)
        {
            Claim pClmEmail = pCPlCurrentUser.FindFirst(ClaimTypes.Email);
            if (pClmEmail != null)
            {
                String pStrEmail = pClmEmail.Value;
                return (pStrEmail);
            }
            else
            {
                return ("Email claim not present.");
            }
        }
        else
        {
            return ("No claims principal present.");
        }
    }
}