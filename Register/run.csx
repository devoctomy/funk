#r "devoctomy.funk.core.dll"
#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

using devoctomy.funk.core.Membership;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;

public static HttpResponseMessage Run(HttpRequestMessage req, TraceWriter log)
{
    ClaimsPrincipal pCPlFacebookUser = ClaimsPrincipal.Current;
    string pStrEmail = pCPlFacebookUser.FindFirst(ClaimTypes.Email).Value;
    Storage pStoMembership = new Storage("TableStorageRootURL", "AzureWebJobsStorage", "ServiceInfo");
    User pUsrUser = pStoMembership.GetUser(pCPlFacebookUser);

    if(pUsrUser == null)
    {
        JObject pJOtCreateUser = new JObject();
        pJOtCreateUser.Add("Email", new JValue(pStrEmail));
        pStoMembership.QueueMessage(pJOtCreateUser.ToString(), "userspending");
        return(req.CreateResponse(HttpStatusCode.OK, $"Registering new user '{pStrEmail}'."));
    }
    else
    {
        return(req.CreateResponse(HttpStatusCode.Conflict, $"User '{pStrEmail}' already registered."));
    }
}