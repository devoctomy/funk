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
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

public static void Run(string myQueueItem, out string outputQueueItem, TraceWriter log)
{
    outputQueueItem = String.Empty;

    JObject pJOtQueueData = JObject.Parse(myQueueItem);
    String pStrEmail = pJOtQueueData["Email"].Value<String>();

    Storage pStoMembership = new Storage("TableStorageRootURL", "AzureWebJobsStorage", "CreateAccount");
    User pUsrUser = pStoMembership.GetUser(GetTestUserPrincipal(pStrEmail));

    if (pUsrUser == null)
    {
        pUsrUser = new User(GetTestUserPrincipal(pStrEmail), 6);

        if (pStoMembership.InsertUser(pUsrUser))
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


private static ClaimsPrincipal GetTestUserPrincipal(String iEmail)
{
    GenericIdentity pGIyTestUser = new GenericIdentity("John Doe");
    pGIyTestUser.AddClaim(new Claim(ClaimTypes.Email, iEmail));
    ClaimsPrincipal pCPlTestUser = new ClaimsPrincipal(pGIyTestUser);
    return (pCPlTestUser);
}