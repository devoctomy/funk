#r "devoctomy.funk.core.dll"

using devoctomy.funk.core.Azure.Functions;
using devoctomy.funk.core.Cryptography;
using devoctomy.funk.core.Environment;
using devoctomy.funk.core.Membership;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Net;
using System.Web;
using System;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("Getting current authenticated users email address.");
    String pStrEmail = await GetAuthenticatedUserEmailAddressAsync(req);

    log.Info("Getting query arguments.");
    String pStrActivationCode = req.GetQueryNameValuePairs().FirstOrDefault(q => string.Compare(q.Key, "activationcode", true) == 0).Value;
    String pStrUserName = req.GetQueryNameValuePairs().FirstOrDefault(q => string.Compare(q.Key, "username", true) == 0).Value;

    log.Info("Initialising membership storage.");
    Storage pStoMembership = new Storage("TableStorageRootURL", "AzureWebJobsStorage", "ActivateAccount");
        
    log.Info($"Getting registered user associated with '{pStrEmail}'.");
    ClaimsPrincipal pCPlUser = CreateUserPrincipal(pStrEmail);
    User pUsrUser = pStoMembership.GetUser(pCPlUser);

    if(pUsrUser == null)
    {
        log.Info("No associated user was found.");

        return(req.CreateResponse(HttpStatusCode.MethodNotAllowed, $"Unknown user '{pStrEmail}'."));
    }
    else
    {
        log.Info("Associated user exists, checking existing activation status.");
        if(!pUsrUser.Activated)
        {
            log.Info($"Not yet activated, attempting activation with username '{pStrUserName}'.");
            Boolean pBlnUserNameTaken = false;
            if(pUsrUser.Activate(pStoMembership, 
                pStrActivationCode,
                pStrUserName,
                out pBlnUserNameTaken))
            {
                log.Info("User successfully activated.");
                return(req.CreateResponse(HttpStatusCode.OK, "User successfully activated."));
            }
            else
            {  
                log.Info("Failed to activate user.");
                if(pBlnUserNameTaken) log.Info("Username already taken.");
                return(req.CreateResponse(HttpStatusCode.MethodNotAllowed, "Failed to activate user."));
            }
        }
        else
        {
            log.Info("User already activated.");
            return(req.CreateResponse(HttpStatusCode.MethodNotAllowed, "User already activated."));
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