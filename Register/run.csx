#r "devoctomy.funk.core.dll"

using devoctomy.funk.core.Azure.Functions;
using devoctomy.funk.core.Membership;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("Getting current authenticated users email address.");
    String pStrEmail = await GetAuthenticatedUserEmailAddressAsync(req);

    log.Info("Initialising membership storage.");
    Storage pStoMembership = new Storage("TableStorageRootURL", "AzureWebJobsStorage", "ServiceInfo");

    log.Info($"Getting registered user associated with '{pStrEmail}'.");
    ClaimsPrincipal pCPlUser = CreateUserPrincipal(pStrEmail);
    User pUsrUser = pStoMembership.GetUser(pCPlUser);

    if(pUsrUser == null)
    {
        log.Info("No associated user was found.");

        log.Info("Queuing user creation.");
        JObject pJOtCreateUser = new JObject();
        pJOtCreateUser.Add("Email", new JValue(pStrEmail));
        pStoMembership.QueueMessage(pJOtCreateUser.ToString(), "userspending");
        return(req.CreateResponse(HttpStatusCode.OK, $"Registering new user '{pStrEmail}'."));
    }
    else
    {
        log.Info("Associated user already exists.");

        return(req.CreateResponse(HttpStatusCode.Conflict, $"User '{pStrEmail}' already registered."));
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
    if(req.Headers.Contains("x-zumo-auth"))
    {
        String pStrXZumoAuth = req.Headers.GetValues("x-zumo-auth").First();
        String pStrEmail = await FunctionsHelpers.GetAuthProviderClaim(String.Format("https://{0}/{1}", req.RequestUri.Host, ".auth/me"),
            pStrXZumoAuth,
            ClaimTypes.Email);
        return(pStrEmail);
    }
    else
    {
        ClaimsPrincipal pCPlCurrentUser = ClaimsPrincipal.Current;
        if(pCPlCurrentUser != null)
        {
            Claim pClmEmail = pCPlCurrentUser.FindFirst(ClaimTypes.Email);
            if(pClmEmail != null)
            {
                String pStrEmail = pClmEmail.Value;
                return(pStrEmail);
            }
            else
            {
                return("Email claim not present.");
            }
        }
        else
        {
            return("No claims principal present.");
        }
    }  
}