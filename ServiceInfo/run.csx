#r "devoctomy.funk.core.dll"
#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

using devoctomy.funk.core.Cryptography;
using devoctomy.funk.core.Environment;
using devoctomy.funk.core.Extensions;
using devoctomy.funk.core.Membership;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Web;

public static HttpResponseMessage Run(HttpRequestMessage req, TraceWriter log)
{   
    System.Security.Claims.ClaimsPrincipal pCPlFacebookUser = System.Security.Claims.ClaimsPrincipal.Current;
	String pStrEmail = pCPlFacebookUser.FindFirst(System.Security.Claims.ClaimTypes.Email).Value;
    Storage pStoMembership = new Storage("TableStorageRootURL", "AzureWebJobsStorage", "ServiceInfo");
    User pUsrUser = pStoMembership.GetUser(pStrEmail);

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

    //JObject pJOtCreateUser = new JObject();
    //pJOtCreateUser.Add("Email", new JValue(pStrEmail));
    //pStoMembership.QueueMessage(pJOtCreateUser.ToString(), "userspending");
    //return(req.CreateResponse(HttpStatusCode.MethodNotAllowed, "Unknown user."));

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