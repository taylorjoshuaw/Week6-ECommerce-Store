using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using StricklandPropane.Data;
using StricklandPropane.Models;

namespace StricklandPropane.Controllers
{
    [Authorize]
    public class EmailController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public EmailController(IConfiguration configuration,
            UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Verify(string email, string code)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(email);

            if (user is null || user.EmailConfirmed)
            {
                return RedirectToAction("Login", "Account");
            }

            if (!string.IsNullOrWhiteSpace(code) &&
                (await _userManager.ConfirmEmailAsync(user, code)).Succeeded)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
                await _signInManager.SignInAsync(user, false);

                return RedirectToAction("Index", "Shop");
            }

            if (!user.SentEmailVerification)
            {
                return await SendEmailVerification(email);
            }

            return View("Verify", email);
        }

        [HttpPost]
        [ActionName("Verify")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmailVerification(string email)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(email);

            // Make sure the user exists and hasn't already confirmed their email address
            if (user is null || user.EmailConfirmed)
            {
                return RedirectToAction("Login", "Account");
            }

            // Generate the verification code and link to be sent in the e-mail
            string code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            string verifyLink = Url.Action(nameof(Verify), "Email",
                new { email, code }, "https", HttpContext.Request.Host.Value);

            // Compose the e-mail message to send to the user
            var message = new SendGridMessage()
            {
                From = new EmailAddress("taylor.joshua88@gmail.com", "Strickland Propane"),
                Subject = "Verify Your Strickland Propane Account",
                HtmlContent = $"<h3>Strickland Propane</h3><h4>Please verify your account by clicking the link below:</h4><a href=\"{verifyLink}\">{verifyLink}</a>",
                PlainTextContent = $"Please copy and paste the following link into your browser's address bar: {verifyLink}"
            };

            // TODO(taylorjoshuaw): Find out if there is a way to add the recipient inside
            //                      the initializer above rather than requiring a
            //                      separate method call
            message.AddTo(email, $"{user.FirstName} {user.LastName}");

            // Set up the SendGrid client using the SendGridAPIKey from configuration
            var client = new SendGridClient(_configuration["SendGridAPIKey"]);
            var response = await client.SendEmailAsync(message);

            // Make sure the e-mail was sent successfully
            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                // Update the user's SentEmailVerification property so that we don't allow
                // the GET request to spam inboxes unintentionally
                user.SentEmailVerification = true;
                await _userManager.UpdateAsync(user);
                return View("Verify", email);
            }

            // TODO(taylorjoshuaw): Add logging here
            // Could not send the verification e-mail!!!
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }
}