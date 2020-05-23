using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Northwind.Models;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;

namespace Northwind.Controllers
{
    public class AccountController : Controller
    {
        private INorthwindRepository repository;
        private UserManager<AppUser> userManager;
        private SignInManager<AppUser> signInManager;
        private IEmailSender emailSender;
        //private ILogger logger;

        public AccountController(INorthwindRepository repo, UserManager<AppUser> userMgr, SignInManager<AppUser> signInMgr, IEmailSender emailSender)
        {
            repository = repo;
            userManager = userMgr;
            signInManager = signInMgr;
            this.emailSender = emailSender;
        }

        // Validate the email address.  Return true or false
        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public IActionResult Login(string returnUrl)
        {
            // return url remembers the user's original request
            ViewBag.returnUrl = returnUrl;
            return View();
        }

        public ViewResult AccessDenied() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel details, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await userManager.FindByEmailAsync(details.Email);
                if (user != null)
                {
                    await signInManager.SignOutAsync();
                    Microsoft.AspNetCore.Identity.SignInResult result = await signInManager.PasswordSignInAsync(user, details.Password, false, false);
                    if (result.Succeeded)
                    {
                        return Redirect(returnUrl ?? "/");
                    }
                }
                ModelState.AddModelError(nameof(LoginModel.Email), "Invalid user or password");
            }
            return View(details);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SendPasswordReset(LoginModel details)
        {
            if ((details.Email == null) || (IsValidEmail(details.Email) == false))
            {
                ModelState.AddModelError(nameof(LoginModel.Email), "A valid user email is required.");
                return View(details);
            }
            else
            {
                AppUser user = await userManager.FindByEmailAsync(details.Email);
                if (user != null)
                {
                    // Send email
                    //logger.LogDebug("Sending password reset confirmation.");
                    var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Request.Scheme + "://" +
                                        Request.Host.Value +
                                        "/Account/PasswordReset?" +
                                        "userId=" + user.Id +
                                        "&code=" + code;

                    await emailSender.SendEmailAsync(details.Email, "Confirm your password reset",
                        $"Please confirm your password reset by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                    // End Send Email
                }
                else
                {
                    return View(details);
                }
            }
            // This should really take the user to a page that gives them instructions
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> PasswordReset(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            code = System.Text.Encoding.UTF8.GetString(Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(code));
            var result = await userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded == true)
            {
                LoginModel theModel = new LoginModel();
                theModel.Email = user.Email;
                return View(theModel);
            }
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> PasswordUpdate(LoginModel details)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await userManager.FindByEmailAsync(details.Email);
                if (user != null)
                {
                    // Validate the password meets the requirements set up in startup.cs
                    var PassordValid = (await userManager.PasswordValidators[0].ValidateAsync(userManager, user, details.Password)).Succeeded;
                    if (PassordValid == false)
                    {
                        ViewBag.message = "Incorrect password format.";
                        details.Password = null;
                        return View("PasswordReset", details);
                    }

                    // compute the new hash string
                    var newPasswordHash = userManager.PasswordHasher.HashPassword(user, details.Password);
                    user.PasswordHash = newPasswordHash;

                    var result = await userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Login", "Account");
                    }
                    ViewBag.message = "Failed to update password.";
                    details.Password = null;
                    return View("PasswordReset", details);
                }
            }
            ViewBag.message = "Password can not be empty.";
            return View("PasswordReset", details);
        }

        private object ValidateAsync(object manager, AppUser user, object password)
        {
            throw new NotImplementedException();
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (IsValidEmail(userId) == false)
            {
                return RedirectToAction("Index", "Home");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // This should be a logged error
                return RedirectToAction("Index", "Home");
            }

            code = System.Text.Encoding.UTF8.GetString(Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(code));
            var result = await userManager.ConfirmEmailAsync(user, code);
            string StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
            return RedirectToAction("Login", "Account");
        }
    }

    public class CustomPasswordValidator<TUser> : PasswordValidator<TUser>, IPasswordValidator<TUser> where TUser : class
    {
        public override Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            //built in identity options
            //var builtIn = base.ValidateAsync(manager, user, password).Result;
            // add your own logic to check the password and append it to builtIn
            return base.ValidateAsync(manager, user, password);
        }
    }
}