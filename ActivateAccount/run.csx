#r "devoctomy.funk.core.dll"
#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

using devoctomy.funk.core;
using devoctomy.funk.core.Environment;
using devoctomy.funk.core.Membership;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    Dictionary<String, String> pDicParams = GetQueryStrings (req);

    String pStrEmail = pDicParams["email"];
    String pStrActivationCode = pDicParams["activationcode"];

    Storage pStoMembership = new Storage(EnvironmentHelpers.GetEnvironmentVariable("StorageRootURL"), "AzureWebJobsStorage");
    User pUsrUser = await pStoMembership.GetUserAsync(pStrEmail);
    if(pUsrUser != null)
    {
        if(await pUsrUser.Activate(pStoMembership, pStrActivationCode))
        {
            SessionToken pSTnToken = new SessionToken(pStrEmail,
                new TimeSpan(1,0,0));
            String pStrJSON = pSTnToken.ToString(Newtonsoft.Json.Formatting.None,
                true,
                EnvironmentHelpers.GetEnvironmentVariable("PrivateRSAKey"));
            HttpResponseMessage pHRMResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            pHRMResponse.Content = new StringContent(pStrJSON);
            return(pHRMResponse);
        }
        else
        {
            return(new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized));
        }
    }
    else
    {
        return(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
    }
}

public static Dictionary<String, String> GetQueryStrings(HttpRequestMessage iRequest)
{
    return(iRequest.GetQueryNameValuePairs().ToDictionary(kv => kv.Key, kv=> kv.Value, StringComparer.OrdinalIgnoreCase));
}