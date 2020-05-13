# .Net Web Development Project

This is the final project for the ***.Net Web Development course 152-109-20086-20*** by Jerry Chiu and Ken Kazinski.

## Source Information

We utilized the information from the microsoft website article *[Create a secure ASP.NET MVC 5 web app with log in, email confirmation and password reset (C#)](https://docs.microsoft.com/en-us/aspnet/mvc/overview/security/create-an-aspnet-mvc-5-web-app-with-email-confirmation-and-password-reset)* to modifiy the module 13 Northwind API project.

## Requirements

1. Password reset complete with email or text verification with a unique code (you will need to research how to send email or text messages).
1. Make sure your password reset feature is secure. *For example*, if you email a
link to reset the password, it should expire after a predefined amount of time, and it should not work more than once. You will need to modify the database schema in order to accomplish this.

As part of the project we are also going to implement the email confirmation portion from the source article.

## Database Update

The first task is to update the database tables.  The chapter 13 table configuration:

![Chapter 13 table view](Documentation\AppIdentityTables.JPG "Chapter 13 Table View")

The **AspNetUsers Table**  contains the required field, *EmailConfirmed*, that will be utilized to confirm the user has confirmed their email.

![AspNetUsers Table](Documentation\AspNetUsersTable.JPG "AspNetUsers Table")

## Why email confirmation

Confirmation of an email account is considered a best practice.

1. Verifies the user is not  impersonating someone else.
2. Validates the email was correct entered.
3. Allows for a password recovery mechanism

**Note:**  Email confirmation provides only limited protection from bots and doesn't provide protection from determined spammers, as they have many working email aliases they can use to register.

We will not allow a user to login until they been confirmed by email, a SMS text message or another mechanism.

## SendGrid

From the webpage *[How to Send Email Using SendGrid with Azure](https://docs.microsoft.com/en-us/azure/sendgrid-dotnet-how-to-send-email)*:

SendGrid is a [cloud-based email service](https://sendgrid.com/solutions) that provides reliable [transactional email delivery](https://sendgrid.com/use-cases/transactional-email), scalability, and real-time analytics along with flexible APIs that make custom integration easy. Common SendGrid use cases include:

- Automatically sending receipts or purchase confirmations to customers.
- Administering distribution lists for sending customers monthly fliers and promotions.
- Collecting real-time metrics for things like blocked email and customer engagement.
- Forwarding customer inquiries.
- Processing incoming emails.

### Create a SendGrid Account

1. Sign into the [Azure portal](https://portal.azure.com/#home).
1. Select create a resource.

![Create A Resource](Documentation\AzureCreateResource.JPG "Azure Create Resource")

3. Search for the SendGrid resource.

![Search for SendGrid](Documentation\AzureSearchForResource.JPG "Search for SendGrid")

![Search for SendGrid](Documentation\AzureSearchForResourceSendGrid.JPG "SendGrid Search")

4. Click the create button.

![Deployment Underway](Documentation\AzureSendGridDeploymentUnderway.JPG "Deployment Underway")

5. Wait for the deployment to complete.

![Deployment Complete](Documentation\AzureSendGridDeploymentComplete.JPG "Deployment Complete")

6. The SendGrid resource will be displayed on the Azure home page.

## Configure SendGrid

The instructions on the *[Create a secure ASP.NET MVC 5 web app with log in, email confirmation and password reset (C#)](https://docs.microsoft.com/en-us/aspnet/mvc/overview/security/create-an-aspnet-mvc-5-web-app-with-email-confirmation-and-password-reset)* page are not current and updated instructions are located at *[Account confirmation and password recovery in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-3.1&tabs=visual-studio#configure-email-provider)*.

## Configure a eMail Provider

The tutorial recommends using *SendGrid* as instead of *SMTP*.  The tutorial states that *SMTP* is difficult to configure and secure correctly.

Create a class to fetch the secure email key. For this sample, create Services/*AuthMessageSenderOptions.cs*:

```
public class AuthMessageSenderOptions
{
    public string SendGridUser { get; set; }
    public string SendGridKey { get; set; }
}
```

## Configure SendGrid user secrets

Our project is **not** going to use the secrets manager and we will configure the service in the *startup.cs* file.

## Install SendGrid

From the package manager console type:
```
Install-Package SendGrid
```

or if using the .NET Command Line Interface (CLI):
```
dotnet add package SendGrid
```

or from the NuGet Package Manager:

![NuGet Package Manager SendGrid](Documentation\NuGetPackageManager_SendGrid.JPG "NuGet Package Manager SendGrid")

