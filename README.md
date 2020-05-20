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

``` C#
public class AuthMessageSenderOptions
{
    public string SendGridUser { get; set; }
    public string SendGridKey { get; set; }
}
```

## Configure SendGrid user secrets

Our project is **not** going to use the secrets manager and we will configure the service in the *Startup.cs* file.

## Install SendGrid

From the package manager console type:

``` .NET Core CLI
Install-Package SendGrid
```

or if using the .NET Command Line Interface (CLI):

``` .NET Core CLI
dotnet add package SendGrid
```

or from the NuGet Package Manager:

![NuGet Package Manager SendGrid](Documentation\NuGetPackageManager_SendGrid.JPG "NuGet Package Manager SendGrid")

## Implement IEmailSender

To Implement IEmailSender, create *Services/EmailSender.cs* with code similar to the following:

``` C#
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Northwind.Services
{
    public class EmailSender : IEmailSender
    {
        public EmailSender(IOptions<AuthMessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        public AuthMessageSenderOptions Options { get; } //set only via Secret Manager

        public Task SendEmailAsync(string email, string subject, string message)
        {
            return Execute(Options.SendGridKey, subject, message, email);
        }

        public Task Execute(string apiKey, string subject, string message, string email)
        {
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("Joe@contoso.com", Options.SendGridUser),
                Subject = subject,
                PlainTextContent = message,
                HtmlContent = message
            };
            msg.AddTo(new EmailAddress(email));

            // Disable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            msg.SetClickTracking(false, false);

            return client.SendEmailAsync(msg);
        }
    }
}
```

Update the *Joe@contoso.com* to your email.

## Configure startup to support email

Add the following code to the ConfigureServices method in the *Startup.cs* file:

- Add EmailSender as a transient service.
- Register the AuthMessageSenderOptions configuration instance.

Add the following using statements to the *Startup.cs*:

``` C#
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Northwind.Services;
```

Add the transient service to the statements to the *Startup.cs*:

``` C#
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<IEmailSender, EmailSender>();
    services.Configure<AuthMessageSenderOptions>(Configuration);

}
```

## Create SendGrid API Key

1. From the Azure SendGrid Accounts page, click *Manage*.

![SendGrid Manage](Documentation\SendGrid_Manage.JPG "SendGrid Manage")

2. In the left menu dashboard select *Settings* and then *API Keys*

![SendGrid Settings](Documentation\SendGrid_Settings.JPG "SendGrid Settings")

3. After the *API Keys* screen appears, select ***Create API Key***.

![SendGrid Create API](Documentation\SendGrid_CreateAPI.JPG "SendGrid Create API")

4. On the *Create API Key* screen enter the API Key Name (e.g. ***NorthwindKey***) and select *Create & View*.

![SendGrid Create & View](Documentation\SendGrid_CreateView.JPG "SendGrid Create & View")

5. The created API Key will be displayed.  Copy the API key and keep it in a safe place.

![SendGrid View Key](Documentation\SendGrid_ViewKey.JPG "SendGrid View Key")

## Email Activity Timeout

The default inactivity time out is *14 days*. To change the default to *10* days use the **Configure Application Cookie** service in the *Startup.cs* file and add a reference to the *system* namespace.

``` C#
using system;

services.ConfigureApplicationCookie(o => {
    o.ExpireTimeSpan = TimeSpan.FromDays(10);
    o.SlidingExpiration = true;
});
```

## Change All Data Protection Token Lifespans

From a security standpoint, tokens should always be set to the shortest interval that allows for good user experience and security.  Tokens should always have an expiration period, this helps avoid replay attacks.

Change all data protection time out to *4 hours* in the *Startup.cs* file's *ConfigureServices* method.

``` C#
    services.Configure<DataProtectionTokenProviderOptions>(o =>
       o.TokenLifespan = TimeSpan.FromHours(4));
```

## Change the Email Token Lifespan and Add a Custom Service Container

The default email token lifespan is *one day*.  To change this value two custom classes, *DataProtectorTokenProvider* and *DataProtectionTokenProviderOptions*  need to be added.  These classes can be added in their own files or to the *Startup.cs* file.

Add the following using statements:

``` C#
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
```

The Northwind API project did not use logging but the *CustomEmailConfirmationTokenProvider* class has a logging element to help with troubleshooting.

Add the classes to the end of the *Startup.cs* file.

``` C#
public class CustomEmailConfirmationTokenProvider<TUser>  : DataProtectorTokenProvider<TUser> where TUser : class
{
    public CustomEmailConfirmationTokenProvider(IDataProtectionProvider dataProtectionProvider,
        IOptions<EmailConfirmationTokenProviderOptions> options,
        ILogger<DataProtectorTokenProvider<TUser>> logger)
        : base(dataProtectionProvider, options)
    {
    }
}

public class EmailConfirmationTokenProviderOptions : DataProtectionTokenProviderOptions
{
    public EmailConfirmationTokenProviderOptions()
    {
        Name = "EmailDataProtectorTokenProvider";
        TokenLifespan = TimeSpan.FromHours(4);
    }
}
```

The article uses the *AddDefaultIdentiy* service:

``` C#
services.AddDefaultIdentity<IdentityUser>(config =>
{
    config.SignIn.RequireConfirmedEmail = true;
    config.Tokens.ProviderMap.Add("CustomEmailConfirmation",
    new TokenProviderDescriptor(
        typeof(CustomEmailConfirmationTokenProvider<IdentityUser>)));
    config.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
}).AddEntityFrameworkStores<ApplicationDbContext>();
```

The Northwind API project uses the *AddIdentity* service.  Update the *AddIdentity* as follows:

``` C#
services.AddIdentity<AppUser, IdentityRole>(opts =>
{
    opts.User.RequireUniqueEmail = true;
    opts.Password.RequiredLength = 6;
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequireLowercase = false;
    opts.Password.RequireUppercase = false;
    opts.Password.RequireDigit = false;
    opts.Password.RequiredUniqueChars = 1;
    opts.SignIn.RequireConfirmedEmail = true;
    opts.Tokens.ProviderMap.Add("CustomEmailConfirmation", 
        new TokenProviderDescriptor(typeof(CustomEmailConfirmationTokenProvider<IdentityUser>)));
    opts.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
}).AddEntityFrameworkStores<AppIdentityDbContext>().AddDefaultTokenProviders();
```

## Register, Confirm Email, and Reset Password Testing

If the project was created using the commands below, a web app with authentication using razor pages would have been created.

```.NET Core CLI
dotnet new webapp -au Individual -uld -o WebPWrecover
cd WebPWrecover
dotnet run
```

The project could have been tested, but as this project used the *Northwind database class project* the project does **not** send the confirmation email and a few more steps will be required to "wire up" the project.

### Changes to *Startup.cs*

The example code uses the *IdentityUser* class but the Northwind API project created the *AppUser* class.  Change the *IdentityUser* to *AppUser*:

``` C#
new TokenProviderDescriptor(typeof(CustomEmailConfirmationTokenProvider<AppUser>)));
opts.Tokens.EmailConfirmationTokenProvider = "CustomEmailConfirmation";
}).AddEntityFrameworkStores<AppIdentityDbContext>().AddDefaultTokenProviders();
```

``` C#
services.AddTransient<CustomEmailConfirmationTokenProvider<AppUser>>();
```

In the *AuthMessageSenderOptions* services configuration, explicitly tell the *Configuration* method which section of the *appsettings.json* to read.

``` C#
services.Configure<AuthMessageSenderOptions>(Configuration.GetSection("Data:SendGrid"));
```

### Changes to *CustomerController.cs*

Add the following *using* statements:

``` C#
using Microsoft.AspNetCore.WebUtilities;
using Northwind.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
```

Add the following private classes:

``` C#
private IEmailSender emailSender;
private ILogger logger;
```

Update the *CustomerController* constructor:

``` C#
public CustomerController(INorthwindRepository repo, UserManager<AppUser> usrMgr, IEmailSender emailSender, ILogger<CustomerController> logger)
{
    repository = repo;
    userManager = usrMgr;
    this.emailSender = emailSender;
    this.logger = logger;
}
```

Remove the requirement for the company name to be unique, as we are using the customers eamil, by deleting the following code:

``` C#
if (repository.Customers.Any(c => c.CompanyName == customer.CompanyName))
{
    ModelState.AddModelError("", "Company Name must be unique");
}
else
{
    ...
}
```

Currently, the code does not send an email.  Add the following code to send the email prior to  writting the new customer to the database.

``` C#
// Send email
logger.LogDebug("User created a new account with password.");

logger.LogDebug("Sending email confirmation.");
var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
code = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(code));

var callbackUrl = Request.Scheme + "://" +
                  Request.Host.Value +
                  "/Account/ConfirmEmail?" +
                  "userId=" + user.Id +
                  "&code=" + code;

await emailSender.SendEmailAsync(customer.Email, "Confirm your email",
    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
// End Send Email

// Create customer (Northwind)
logger.LogDebug("Adding user to database.");
repository.AddCustomer(customer);
return RedirectToAction("Index", "Home");
```

The *callbackUrl* variable contains the *URL* where the confirmation is sent to.

The base *URL* is creted by:

``` C#
Request.Scheme + "://" + Request.Host.Value + "/Account/ConfirmEmail?"
```

Resulting in ***http://localhost:50230/Account/ConfirmEmail***

There are two parameters, *userId* and *code*:

``` C#
"userId=" + user.Id + "&code=" + code
```

Resulting in ***?userId=\<Value\>&code=\<Value\>***

### Database Entry

The database now contains the new customer but the *EmailConfirmed* field is set to *false*

![Register Customer](Documentation\RegisterCustomer.JPG "Register Customer")

### Add destination method to the *AccountController.cs*

The confirmation email contains a link to confirm the customers email address.

![Conformation eMail](Documentation\ConfirmationEmail.JPG "Conformation eMail")

The route listed in the sending email  ***http://localhost:50230/Account/ConfirmEmail***.  There needs to be a needs to have a method in the *AccountController* that contains two paramaters to process the returning information.

``` C#
public async Task<IActionResult> ConfirmEmail(string userId, string code)
{
    if (userId == null || code == null)
    {
        return RedirectToAction("Index", "Home");
    }

    var user = await userManager.FindByIdAsync(userId);
    if (user == null)
    {
        return NotFound($"Unable to load user with ID '{userId}'.");
    }

    code = System.Text.Encoding.UTF8.GetString(Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(code));
    var result = await userManager.ConfirmEmailAsync(user, code);
    string StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
    return RedirectToAction("Account", "Login");
}
```

Until the user confirms the account registration, they will not be able to log in.  Currently the code simply states the *user name* or *password* is incorrect.

![Conformation eMail Not Recieved](Documentation\ConfirmationEmilNotReceived.JPG "Conformation eMail Not Recieved")

When the user clicks the confirmation, the database is updated to *True*.

![Registered Customer Confirmed](Documentation\RegisterCustomerConfirmed.JPG "Confirmed Registered Customer")

and the user is redirected to the login page.

```C#
return RedirectToAction("Account", "Login");
```

![Login Page](Documentation\LogIn.JPG "Login Page")
