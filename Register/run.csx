#r "devoctomy.funk.core.dll"

using devoctomy.funk.core.Membership;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;

public static HttpResponseMessage Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info("Getting current authenticated user ClaimPrincipal.");
    ClaimsPrincipal pCPlFacebookUser = ClaimsPrincipal.Current;
    String pStrEmail = pCPlFacebookUser.FindFirst(ClaimTypes.Email).Value;

    log.Info("Initialising membership storage.");
    Storage pStoMembership = new Storage("TableStorageRootURL", "AzureWebJobsStorage", "ServiceInfo");

    log.Info($"Getting registered user associated with '{pStrEmail}'.");
    User pUsrUser = pStoMembership.GetUser(pCPlFacebookUser);

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