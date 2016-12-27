#r "devoctomy.funk.core.dll"

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
    log.Info("Getting query arguments.");
    String pStrActivationCode = req.GetQueryNameValuePairs().FirstOrDefault(q => string.Compare(q.Key, "activationcode", true) == 0).Value;
    String pStrUserName = req.GetQueryNameValuePairs().FirstOrDefault(q => string.Compare(q.Key, "username", true) == 0).Value;

    log.Info("Getting current authenticated user ClaimPrincipal.");
    ClaimsPrincipal pCPlFacebookUser = ClaimsPrincipal.Current;
	String pStrEmail = pCPlFacebookUser.FindFirst(System.Security.Claims.ClaimTypes.Email).Value;
 
    log.Info("Initialising membership storage.");
    Storage pStoMembership = new Storage("TableStorageRootURL", "AzureWebJobsStorage", "ServiceInfo");
        
    log.Info($"Getting registered user associated with '{pStrEmail}'.");
    User pUsrUser = pStoMembership.GetUser(pCPlFacebookUser);

    if(pUsrUser == null)
    {
        log.Info("No associated user was found.");

        return(req.CreateResponse(HttpStatusCode.MethodNotAllowed, "Unknown user."));
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