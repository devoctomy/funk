#r "devoctomy.funk.core.dll"

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

    log.Info("Parsing queue message.");
    JObject pJOtQueueData = JObject.Parse(myQueueItem);
    String pStrEmail = pJOtQueueData["Email"].Value<String>();

    log.Info("Initialising membership storage.");
    Storage pStoMembership = new Storage("TableStorageRootURL", "AzureWebJobsStorage", "CreateAccount");

    log.Info($"Getting registered user associated with '{pStrEmail}'.");
    User pUsrUser = pStoMembership.GetUser(GetTestUserPrincipal(pStrEmail));

    if(pUsrUser == null)
    {
        log.Info("No associated user was found, creating one.");
        pUsrUser = new User(GetTestUserPrincipal(pStrEmail), 6);

        log.Info("Inserting user into table storage.");
        if (pStoMembership.InsertUser(pUsrUser))
        {
            log.Info("Queuing new user's welcome email.");

            JObject pJOtEmail = new JObject();
            pJOtEmail.Add("From", new JValue(EnvironmentHelpers.GetEnvironmentVariable("FromEmailAddress")));
            pJOtEmail.Add("To", new JValue(pStrEmail));
            pJOtEmail.Add("Subject", new JValue(EnvironmentHelpers.GetEnvironmentVariable("WelcomeEmailSubject")));
            pJOtEmail.Add("Message", new JValue($"Welcome to {EnvironmentHelpers.GetEnvironmentVariable("AppName")}. Your activation code is {pUsrUser.ActivationCode}.")); 
            outputQueueItem = pJOtEmail.ToString();
        }
        else
        {
            log.Info("Failed to insert user...");
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