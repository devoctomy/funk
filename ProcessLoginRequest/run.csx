#r "devoctomy.funk.core.dll"
#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"
#r "SendGrid"

using devoctomy.funk.core;
using devoctomy.funk.core.Environment;
using devoctomy.funk.core.Membership;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

public async static Task Run(string myQueueItem, TraceWriter log)
{
    JObject pJOtQueueData = JObject.Parse(myQueueItem);
    String pStrEmail = pJOtQueueData["Email"].Value<String>();

    Storage pStoMembership = new Storage(EnvironmentHelpers.GetEnvironmentVariable("StorageRootURL"), "AzureWebJobsStorage");
    User pUsrUser = await pStoMembership.GetUserAsync(pStrEmail);
    if(pUsrUser != null)
    {
        if(await pUsrUser.RandomiseOTP(pStoMembership, 6))
        {
            //Change to sendgrid output
            QueueEmail(EnvironmentHelpers.GetEnvironmentVariable("FromEmailAddress"), 
                pStrEmail,
                EnvironmentHelpers.GetEnvironmentVariable("LoginRequestEmailSubject"),
                $"Your one time password is {pUsrUser.OTP}.");
        }
    }
}

private static void QueueEmail(String iFrom, 
    String iTo, 
    String iSubject, 
    String iMessage)
{
    JObject pJOtEmail = new JObject();
    pJOtEmail.Add("From", new JValue(iFrom));
    pJOtEmail.Add("To", new JValue(iTo));
    pJOtEmail.Add("Subject", new JValue(iSubject));
    pJOtEmail.Add("Message", new JValue(iMessage));    
 
    String pStrConnectionString = EnvironmentHelpers.GetEnvironmentVariable("AzureWebJobsStorage");
    CloudStorageAccount pCSAAccount = CloudStorageAccount.Parse(pStrConnectionString);
    CloudQueueClient pCQCClient = pCSAAccount.CreateCloudQueueClient();
    CloudQueue pCQeQueue = pCQCClient.GetQueueReference("emailoutbox");
    pCQeQueue.CreateIfNotExists();
    String pStrEmail = pJOtEmail.ToString(Newtonsoft.Json.Formatting.None);
    CloudQueueMessage pCQMEmail = new CloudQueueMessage(pStrEmail);
    pCQeQueue.AddMessage(pCQMEmail);
}