#r "devoctomy.funk.core.dll"

using devoctomy.funk.core.Cryptography;
using devoctomy.funk.core.Environment;
using devoctomy.funk.core.Extensions;
using devoctomy.funk.core.Membership;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Net;
using System.Web;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{   
    log.Info("Getting current authenticated user ClaimPrincipal.");
    ClaimsPrincipal pCPlFacebookUser = ClaimsPrincipal.Current;
    String pStrEmail = pCPlFacebookUser.FindFirst(ClaimTypes.Email).Value;

    log.Info("Initialising membership storage.");
    Storage pStoMembership = new Storage("TableStorageRootURL", "AzureWebJobsStorage", "ServiceInfo");   

    log.Info($"Getting registered user associated with '{pStrEmail}'.");
    User pUsrUser = pStoMembership.GetUser(pCPlFacebookUser);

    if(pUsrUser != null)
    {
        log.Info("Associated user exists, checking existing activation status.");
        if(!pUsrUser.Activated)
        {
            log.Info("User not activated.");
            return(req.CreateResponse(HttpStatusCode.MethodNotAllowed, "User not activated."));
        }
    }
    else
    {
        log.Info("No associated user was found.");

        return(req.CreateResponse(HttpStatusCode.MethodNotAllowed, "Unknown user."));
    }

    log.Info("Returning service information.");
    JObject pJOtInfo = new JObject();
    pJOtInfo.Add("Name", new JValue(EnvironmentHelpers.GetEnvironmentVariable("AppName")));
    pJOtInfo.Add("Version", new JValue("1.0.0.0"));
    pJOtInfo.Add("InfoRequestedAt", new JValue(DateTime.UtcNow.ToString(EnvironmentHelpers.GetEnvironmentVariable("DateTimeFormat"))));
    pJOtInfo.Add("PublicKey", new JValue(EnvironmentHelpers.GetEnvironmentVariable("PublicRSAKey")));
    pJOtInfo.Add("FacebookCallbackURL", new JValue(String.Format("https://{0}/.auth/login/facebook/callback", req.RequestUri.Host)));

    RSAParametersSerialisable pRSAPrivate = RSAParametersSerialisable.FromJSON(EnvironmentHelpers.GetEnvironmentVariable("PrivateRSAKey"),
        true);
    pRSAPrivate.Sign(pJOtInfo);

    HttpResponseMessage pHRMResponse = new HttpResponseMessage(HttpStatusCode.OK);
    pHRMResponse.Content = new StringContent(pJOtInfo.ToString(Newtonsoft.Json.Formatting.None));
    return(pHRMResponse);
}