#r "Newtonsoft.Json"
#r "SendGrid"

using Newtonsoft.Json.Linq;
using System;
using SendGrid;
using SendGrid.Helpers.Mail;

public static void Run(string myQueueItem, TraceWriter log)
{
    JObject pJOtQueueData = JObject.Parse(myQueueItem);
    String pStrFrom = pJOtQueueData["From"].Value<String>();
    String pStrTo = pJOtQueueData["To"].Value<String>();
    String pStrSubject = pJOtQueueData["Subject"].Value<String>();
    String pStrMessage = pJOtQueueData["Message"].Value<String>();

    log.Info($"Sending email from '{pStrFrom}' to '{pStrTo}'.");
    SendEmail(pStrFrom, pStrTo, pStrSubject, pStrMessage);
}

public static string GetEnvironmentVariable(string iName)
{
    return(System.Environment.GetEnvironmentVariable(iName, EnvironmentVariableTarget.Process));
}

private static void SendEmail(String iFrom, 
    String iTo, 
    String iSubject, 
    String iMessage)
{
    String pStrAPIKey = GetEnvironmentVariable("SendGridAPIKey");
    SendGridAPIClient sg = new SendGridAPIClient(pStrAPIKey);
    Email pEmlFrom = new Email(iFrom);
    Email pEmlTo = new Email(iTo);
    Content pConContent = new Content("text/plain", iMessage);
    Mail mail = new Mail(pEmlFrom, iSubject, pEmlTo, pConContent);
    dynamic response = sg.client.mail.send.post(requestBody: mail.Get());
}