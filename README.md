# funk

Azure function implementation of devoctomy funk core framework.

## Purpose

The purpose of this Azure Function app is to provide a set of lightweight functionality that supports the creation of a simple service-based user enrollment system.  The endpints are designed to be used within any mobile app / website.

As well as being lightweight, all functions are designed with scaleability in mind, funk should grow to suit any demand made upon it.

>  funk requires the devoctomy.funk.core library.  Written in C# and also available on GitHub, [here](https://github.com/devoctomy/devoctomy.funk.core)  This repository also contains clients required for interacting with a funk service from an app.

## Required App Settings

| Name | Description | Suggest Value (If Any) |
|--------|:--------:|--------:|
| AzureWebJobsDashboard | ? | ? |
| AzureWebJobsStorage | ? | ? |
| FUNCTIONS_EXTENSION_VERSION | | ~1 |
| WEBSITE_CONTENTAZUREFILECONNECTIONSTRING | ? | ? |
| WEBSITE_CONTENTSHARE | ? | ? |
| WEBSITE_NODE_DEFAULT_VERSION | | 6.5.0 |
| SendGridAPIKey | Your SendGrid API Key | NA |
| DateTimeFormat | Date / Time format used for serialisation of DateTimes | yyyy-MM-ddThh:mm:ssZ |
| PublicRSAKey | Your apps public RSA key as a hex encoded string | NA |
| PrivateRSAKey | Your apps private RSA key as a hex encoded string | NA |
| TableStorageRootURL | The root url of your apps table storage | NA |
| WelcomeEmailSubject | The subject used for welcome emails, sent when the account is created | Activate Your Account |
| AppName | The name of your app | NA |
| FromEmailAddress | The email address used to send emails from using SendGrid | NA |

## Functions Overview

The following is a list of functionality provided by funk.

### ActivateAccount

| Name | Description |
|--------|--------|
| Trigger | HTTP GET |
| Output | Activates a newly created account |
| Parameters | email, activation code |

### CreateAccount

| Name | Description |
|--------|--------|
| Trigger | Azure Queue |
| Output | Creates the user table entry and emails the user an activation code |
| Parameters | email, username |

### Register

| Name | Description |
|--------|--------|
| Trigger | HTTP GET |
| Output | Begins the creation of an account for the authenticated user |
| Parameters | None |

### SendEmail

| Name | Description |
|--------|--------|
| Trigger | Azure Queue |
| Output | Sends an email using SendGrid |
| Parameters | from, to, subject, message |

### ServiceInfo

| Name | Description |
|--------|--------|
| Trigger | HTTP GET |
| Output | JSON Object containing information on the service including the public RSA key |
| Parameters | None |

### UserInfo

| Name | Description |
|--------|--------|
| Trigger | HTTP GET |
| Output | Get status of the authenticated user, such as registration / activation status. |
| Parameters | None |

## Deployment Notes

Deployment of funk is relatively simple, instructions will come in due course for deploying from GitHub, until then please refer to the following basic instructions.

1.  Create Azure Function App and storage account.  Choose the region closest to you to start with.
2.  Deploy the code from this repository using the App Service Editor.
3.  Setup your App Settings as per the information in the "Required App Settings" section above.  During this process, make sure you have a SendGrid account and a valid API key.
4.  Configure authentication, at current I am testing with Facebook authentication.  Make sure that you have "Action to take when request is not authenticated" set to login with your chose provider.  Please note that no functions can be invoked without being authenticated.
5.  Try the "/api/Register" method from your browser, once you have authenticated, an account creation request should be queued and you should get a message saying "Registering new user {email}".  An email should be sent to you with an activation code.
6.  Activate the account via the "/api/ActivateAccount?activationcode={activationcode}" method.  The activation code should be in the email sent to you in the previous step.

Funk has been designed to work from both the browser, and also an authenticated client app.  There is a Windows 10 Universal Client in this repository with a test application that shows you how to authenticate, register, activate and invoke authenticated functions easily.

The registration process is a little different and has the following basic flow,

1.  Authenticate
2.  Invoke UserInfo, this will return the authenticated status of the user.
3.  Invoke Register if 'Registered' in false.
4.  Invoke Activate if 'Activated' is false, this requires a unique username and also the activation code emailed to the registered users email address.

*That's it for now, I'm currently building up methods for profile, data storage etc.*

> Please note:  Debugging authenticated Azure Functions can be a bit of a pain as the in-browser compiler doesn't always work, I find a combination of disabling the authentication to check for compiler errors and also compiling within the Azure Functions Tools for Visual Studio 2017; helps to flush out issues.