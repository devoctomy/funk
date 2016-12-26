#r "devoctomy.funk.core.dll"
#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

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
    String pStrActivationCode = req.GetQueryNameValuePairs().FirstOrDefault(q => string.Compare(q.Key, "activationcode", true) == 0).Value;
    String pStrUserName = req.GetQueryNameValuePairs().FirstOrDefault(q => string.Compare(q.Key, "username", true) == 0).Value;

    ClaimsPrincipal pCPlFacebookUser = ClaimsPrincipal.Current;
	String pStrEmail = pCPlFacebookUser.FindFirst(System.Security.Claims.ClaimTypes.Email).Value;
    Storage pStoMembership = new Storage("TableStorageRootURL", "AzureWebJobsStorage", "ServiceInfo");
    User pUsrUser = pStoMembership.GetUser(pCPlFacebookUser);

    if(pUsrUser == null)
    {
        return(req.CreateResponse(HttpStatusCode.MethodNotAllowed, "Unknown user."));
    }
    else
    {
        if(!pUsrUser.Activated)
        {
            Boolean pBlnUserNameTaken = false;
            if(pUsrUser.Activate(pStoMembership, 
                pStrActivationCode,
                pStrUserName,
                out pBlnUserNameTaken))
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