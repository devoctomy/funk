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
using Newtonsoft.Json.Linq;
using System;
using System.Security.Cryptography;
using System.Text;

public static void Run(string myQueueItem, out string outputQueueItem, TraceWriter log)
{
    outputQueueItem = String.Empty;

    JObject pJOtQueueData = JObject.Parse(myQueueItem);
    String pStrEmail = pJOtQueueData["Email"].Value<String>();
    String pStrUserName = pJOtQueueData["UserName"].Value<String>();
 
    Storage pStoMembership = new Storage(EnvironmentHelpers.GetEnvironmentVariable("StorageRootURL"), "AzureWebJobsStorage");
    User pUsrUser = pStoMembership.GetUser(pStrEmail);
    if(pUsrUser == null)
    {
        pUsrUser = new User(pStrEmail, pStrUserName, 6);

        if(pUsrUser.Insert(pStoMembership))
        {
            JObject pJOtEmail = new JObject();
            pJOtEmail.Add("From", new JValue(EnvironmentHelpers.GetEnvironmentVariable("FromEmailAddress")));
            pJOtEmail.Add("To", new JValue(pStrEmail));
            pJOtEmail.Add("Subject", new JValue(EnvironmentHelpers.GetEnvironmentVariable("CreateAccountEmailSubject")));
            pJOtEmail.Add("Message", new JValue($"Your activation code is {EnvironmentHelpers.GetEnvironmentVariable("AppName")}.")); 

            outputQueueItem = pJOtEmail.ToString();
        }
        else
        {
            log.Info("Failed to inserting user...");
        }
    }
}
