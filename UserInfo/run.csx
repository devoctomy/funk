#r "devoctomy.funk.core.dll"

using devoctomy.funk.core.Azure.Functions;
using devoctomy.funk.core.Cryptography;
using devoctomy.funk.core.Environment;
using devoctomy.funk.core.Membership;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Net;
using System.Web;
using System;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("Getting authentication status.");
    Boolean pBlnAppAuthenticated = req.Headers.Contains("x-zumo-auth");
    JObject pJOtResponse = new JObject();
    pJOtResponse.Add("Authenticated", new JValue(pBlnAppAuthenticated ? "App" : "Browser"));

    log.Info("Getting email.");
    String pStrEmail = await GetAuthenticatedUserEmailAddress(req);
    pJOtResponse.Add("Email", new JValue(pStrEmail));

    String pStrNameIdentifer = String.Empty;
    if(pBlnAppAuthenticated)
    {
        log.Info("Getting NameIdentifier as app authenticated.");
        if(GetNameIdentifier(out pStrNameIdentifer))
        {
            pJOtResponse.Add("NameIdentifier", new JValue(pStrNameIdentifer));
        }
    }

    log.Info("Initialising membership storage.");
    Storage pStoMembership = new Storage("TableStorageRootURL", "AzureWebJobsStorage", "UserInfo");   

    log.Info($"Getting registered user associated with '{pStrEmail}'.");
    User pUsrUser = pStoMembership.FindUser(pStrEmail);

    if(pUsrUser != null)
    {
        log.Info("Associated user exists, getting details.");
        pJOtResponse.Add("Registered", new JValue(true));
        pJOtResponse.Add("Activated", new JValue(pUsrUser.Activated));
        pJOtResponse.Add("Locked", new JValue(pUsrUser.Locked));
    }   
    else
    {        
        log.Info("No associated user was found.");
        pJOtResponse.Add("Registered", new JValue(false));
    }

    HttpResponseMessage pHRMResponse = new HttpResponseMessage(HttpStatusCode.OK);
    pHRMResponse.Content = new StringContent(pJOtResponse.ToString(Newtonsoft.Json.Formatting.None));
    return(pHRMResponse);
}

private static Boolean GetNameIdentifier(out String oNameIdentifier)
{
    oNameIdentifier = String.Empty;
    ClaimsPrincipal pCPlCurrentUser = ClaimsPrincipal.Current;
    if(pCPlCurrentUser != null)
    {
        Claim pClmNameIdentifier = pCPlCurrentUser.FindFirst(ClaimTypes.NameIdentifier);
        if(pClmNameIdentifier != null)
        {
            oNameIdentifier = pClmNameIdentifier.Value;
            return(true);
        }
    }
    return(false);
}

private static async Task<String> GetAuthenticatedUserEmailAddress(HttpRequestMessage req)
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