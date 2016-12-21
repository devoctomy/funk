#r "devoctomy.funk.core.dll"
#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

using devoctomy.funk.core.Membership;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Web;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    System.Security.Claims.ClaimsPrincipal pCPlFacebookUser = System.Security.Claims.ClaimsPrincipal.Current;
	String pStrEmail = pCPlFacebookUser.FindFirst(System.Security.Claims.ClaimTypes.Email).Value;
    Storage pStoMembership = new Storage("TableStorageRootURL", "AzureWebJobsStorage", "ServiceInfo");
    User pUsrUser = pStoMembership.GetUser(pStrEmail);

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