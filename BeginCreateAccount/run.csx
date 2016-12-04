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
using System;
using System.Net;
using Newtonsoft.Json.Linq;

public static HttpResponseMessage Run(HttpRequestMessage req, out string outputQueueItem, TraceWriter log)
{
    outputQueueItem = String.Empty;
    Dictionary<String, String> pDicParams = GetQueryStrings(req);

    String pStrEmail = pDicParams["email"];
    String pStrUserName = pDicParams["username"];

    Storage pStoMembership = new Storage(EnvironmentHelpers.GetEnvironmentVariable("StorageRootURL"), "AzureWebJobsStorage");
    User pUsrUser = pStoMembership.GetUser(pStrEmail);
    if(pUsrUser == null)
    {
        JObject pJOtCreateUser = new JObject();
        pJOtCreateUser.Add("Email", new JValue(pStrEmail));
        pJOtCreateUser.Add("UserName", new JValue(pStrUserName));
        outputQueueItem = pJOtCreateUser.ToString(Newtonsoft.Json.Formatting.None);
        return(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
    }
    else
    {
        return(new HttpResponseMessage(System.Net.HttpStatusCode.Conflict));
    }
}

public static Dictionary<String, String> GetQueryStrings(HttpRequestMessage iRequest)
{
    return(iRequest.GetQueryNameValuePairs().ToDictionary(kv => kv.Key, kv=> kv.Value, StringComparer.OrdinalIgnoreCase));
}