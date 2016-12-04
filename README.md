# funk

Azure function implementation of devoctomy funk core framework.

## Purpose

The purpose of this Azure Function app is to provide a set of lightweight functionality that supports the creation of a simple service-based user enrollment system.  The endpints are designed to be used within any mobile app / website.

As well as being lightweight, all functions are designed with scaleability in mind, funk should grow to suit any demand made upon it.

>  funk requires the devoctomy.funk.core library.  Written in C# and also available on GitHub, [here](https://github.com/devoctomy/devoctomy.funk.core)

## Required App Settings

| Name | Description | Suggest Value (If Any) |
|--------|--------|
| AzureWebJobsDashboard | | |
| AzureWebJobsStorage | | |
| FUNCTIONS_EXTENSION_VERSION | | ~1 |
| WEBSITE_CONTENTAZUREFILECONNECTIONSTRING | | |
| WEBSITE_CONTENTSHARE | | |
| WEBSITE_NODE_DEFAULT_VERSION | | 6.5.0 |
| SendGridAPIKey | Your SendGrid API Key | N/A |
| DateTimeFormat | Date / Time format used for serialisation of DateTimes | yyyy-MM-ddThh:mm:ssZ |
| PublicRSAKey | Your apps public RSA key as a hex encoded string | N/A |
| PrivateRSAKey | Your apps private RSA key as a hex encoded string | N/A |
| StorageRootURL | The root url of your apps storage | N/A |
| CreateAccountEmailSubject | The subject used for account creation emails | Activate Your Account |
| AppName | The name of your app | N/A |
| LoginRequestEmailSubject | The subject used for login request emails | Login Request |
| FromEmailAddress | The email address used to send emails from using SendGrid | N/A |

## Functions Overview

The following is a list of functionality provided by funk.

### ServiceInfo

| Name | Description |
|--------|--------|
| Trigger | HTTP GET |
| Output | JSON Object containing information on the service including the public RSA key |
| Parameters | None |

### BeginCreateAccount

| Name | Description |
|--------|--------|
| Trigger | HTTP GET |
| Output | Invokes the creation of a new user on the system |
| Parameters | email, username |

### CreateAccount

| Name | Description |
|--------|--------|
| Trigger | Azure Queue |
| Output | Creates the user table entry and emails the user an activation code |
| Parameters | email, username |

### ActivateAccount

| Name | Description |
|--------|--------|
| Trigger | HTTP GET |
| Output | Activates a newly created account |
| Parameters | email, activation code |

### RequestLogin

| Name | Description |
|--------|--------|
| Trigger | HTTP GET |
| Output | Requests account login, results in a new one time password being generated and being emailed to the user |
| Parameters | email |

### Login

| Name | Description |
|--------|--------|
| Trigger | HTTP GET |
| Output | Logs in a user and returns a signed session token |
| Parameters | email, one time password |

### ProcessLoginRequest

| Name | Description |
|--------|--------|
| Trigger | HTTP GET |
| Output | Logs in a user and returns a signed session token |
| Parameters | email, one time password |

### SendEmail

| Name | Description |
|--------|--------|
| Trigger | Azure Queue |
| Output | Sends an email using SendGrid |
| Parameters | from, to, subject, message |