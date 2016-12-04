#r "devoctomy.funk.core.dll"
#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

using devoctomy.funk.core;
using devoctomy.funk.core.Environment;
using devoctomy.funk.core.Membership;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Net;
using Newtonsoft.Json.Linq;

public static HttpResponseMessage Run(HttpRequestMessage req, out string outputQueueItem, TraceWriter log)
{
    log.Info("1");
    outputQueueItem = String.Empty;
    Dictionary<String, String> pDicParams = GetQueryStrings(req);

    String pStrEmail = pDicParams["email"];

    Storage pStoMembership = new Storage(EnvironmentHelpers.GetEnvironmentVariable("StorageRootURL"), "AzureWebJobsStorage");
    User pUsrUser = pStoMembership.GetUser(pStrEmail);
    if(pUsrUser != null)
    {
        log.Info("found user.");
        JObject pJOtLoginRequest = new JObject();
        pJOtLoginRequest.Add("Email", new JValue(pStrEmail));
        outputQueueItem = pJOtLoginRequest.ToString(Newtonsoft.Json.Formatting.None);
        return(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
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