#r "devoctomy.funk.core.dll"
#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

using devoctomy.funk.core.Azure.Functions;
using devoctomy.funk.core.Cryptography;
using devoctomy.funk.core.Environment;
using devoctomy.funk.core.Extensions;
using devoctomy.funk.core.Membership;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Web;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    FunctionResponseBase pFRBResponse = new FunctionResponseBase();

    System.Security.Claims.ClaimsPrincipal pCPlFacebookUser = System.Security.Claims.ClaimsPrincipal.Current;
    Storage pStoMembership = new Storage("TableStorageRootURL", "AzureWebJobsStorage", "Profile");
    User pUsrUser = pStoMembership.GetUser(pCPlFacebookUser);

    if(pUsrUser != null)
    {
        if(!pUsrUser.Activated)
        {
            return(req.CreateResponse(HttpStatusCode.MethodNotAllowed, "User not activated."));
        }
    }
    else
    {
        return(req.CreateResponse(HttpStatusCode.MethodNotAllowed, "Unknown user."));
    }

    switch(req.Method.Method)
    {
        case "GET":
        {
            Profile pProProfile = pUsrUser.GetProfile(pStoMembership);
            String pStrRetVal = pFRBResponse.ToJSON(true,
                HttpStatusCode.OK,
                pProProfile,
                Newtonsoft.Json.Formatting.None);

            HttpResponseMessage pHRMResponse = new HttpResponseMessage(HttpStatusCode.OK);
            pHRMResponse.Content = new StringContent(pStrRetVal);
            return(pHRMResponse);
        }
        case "PUT":
        {
            String pStrContent = await req.Content.ReadAsStringAsync();
            Profile pProSource = Profile.FromJSON(pStrContent);
            Profile pProTarget = pUsrUser.GetProfile(pStoMembership);
            pProTarget.SetFrom(pProSource);
            pProTarget.Replace(pStoMembership, pUsrUser);

            return(req.CreateResponse(HttpStatusCode.OK, "Profile updated."));
        }
        default:
        {
            return(req.CreateResponse(HttpStatusCode.BadRequest, "Method not supported."));
        }
    }

    return(req.CreateResponse(HttpStatusCode.OK, "OK."));
}