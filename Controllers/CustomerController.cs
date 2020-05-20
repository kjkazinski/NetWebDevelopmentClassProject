using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Northwind.Models;
using Microsoft.AspNetCore.WebUtilities;
using Northwind.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;

namespace Northwind.Controllers
{
    public class CustomerController : Controller
    {
        // this controller depends on the NorthwindRepository & the UserManager
        private INorthwindRepository repository;
        private UserManager<AppUser> userManager;
        private IEmailSender emailSender;
        private ILogger logger;

        public CustomerController(INorthwindRepository repo, UserManager<AppUser> usrMgr, IEmailSender emailSender, ILogger<CustomerController> logger)
        {
            repository = repo;
            userManager = usrMgr;
            this.emailSender = emailSender;
            this.logger = logger;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<IActionResult> Register(CustomerWithPassword customerWithPassword)
        {
            if (ModelState.IsValid)
            {
                Customer customer = customerWithPassword.Customer;
                if (ModelState.IsValid)
                {
                    AppUser user = new AppUser
                    {
                        // email and username are synced - this is by choice
                        Email = customer.Email,
                        UserName = customer.Email
                    };
                    // Add user to Identity DB
                    IdentityResult result = await userManager.CreateAsync(user, customerWithPassword.Password);
                    if (!result.Succeeded)
                    {
                        AddErrorsFromResult(result);
                    }
                    else
                    {
                        // Assign user to customers Role
                        result = await userManager.AddToRoleAsync(user, "Customer");

                        if (!result.Succeeded)
                        {
                            // Delete User from Identity DB
                            await userManager.DeleteAsync(user);
                            AddErrorsFromResult(result);
                        }
                        else
                        {
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
                        }
                    }
                }
            }
            return View();
        }

        [Authorize(Roles = "Customer")]
        public IActionResult Account() => View(repository.Customers.FirstOrDefault(c => c.Email == User.Identity.Name));

        [Authorize(Roles = "Customer"), HttpPost, ValidateAntiForgeryToken]
        public IActionResult Account(Customer customer)
        {
            // Edit customer info
            repository.EditCustomer(customer);
            return RedirectToAction("Index", "Home");
        }

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
    }
}