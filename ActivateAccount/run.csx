#r "devoctomy.funk.core.dll"
#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

using devoctomy.funk.core.Cryptography;
using devoctomy.funk.core.Environment;
using devoctomy.funk.core.Membership;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Web;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    String pStrActivationCode = req.GetQueryNameValuePairs().FirstOrDefault(q => string.Compare(q.Key, "activationcode", true) == 0).Value;

    System.Security.Claims.ClaimsPrincipal pCPlFacebookUser = System.Security.Claims.ClaimsPrincipal.Current;
	String pStrEmail = pCPlFacebookUser.FindFirst(System.Security.Claims.ClaimTypes.Email).Value;
    Storage pStoMembership = new Storage("TableStorageRootURL", "AzureWebJobsStorage", "ServiceInfo");
    User pUsrUser = pStoMembership.GetUser(pStrEmail);

    if(pUsrUser == null)
    {
        return(req.CreateResponse(HttpStatusCode.MethodNotAllowed, "Unknown user."));
    }
    else
    {
        if(!pUsrUser.Activated)
        {
            if(pUsrUser.Activate(pStoMembership, pStrActivationCode))
            {
                return(req.CreateResponse(HttpStatusCode.OK, "User successfully activated."));
            }
            else
            {
                return(req.CreateResponse(HttpStatusCode.MethodNotAllowed, "Failed to activate user."));
            }
        }
        else
        {
            return(req.CreateResponse(HttpStatusCode.MethodNotAllowed, "User already activated."));
        }
    }
}