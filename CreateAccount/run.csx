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

    Storage pStoMembership = new Storage("TableStorageRootURL", "AzureWebJobsStorage", "CreateAccount");
    User pUsrUser = pStoMembership.GetUser(pStrEmail);
    if(pUsrUser == null)
    {
        pUsrUser = new User(pStrEmail, 6);

        if(pUsrUser.Insert(pStoMembership))
        {
            JObject pJOtEmail = new JObject();
            pJOtEmail.Add("From", new JValue(EnvironmentHelpers.GetEnvironmentVariable("FromEmailAddress")));
            pJOtEmail.Add("To", new JValue(pStrEmail));
            pJOtEmail.Add("Subject", new JValue(EnvironmentHelpers.GetEnvironmentVariable("WelcomeEmailSubject")));
            pJOtEmail.Add("Message", new JValue($"Welcome to {EnvironmentHelpers.GetEnvironmentVariable("AppName")}. Your activation code is {pUsrUser.ActivationCode}.")); 

            outputQueueItem = pJOtEmail.ToString();
        }
        else
        {
            log.Info("Failed to inserting user...");
        }
    }
}
