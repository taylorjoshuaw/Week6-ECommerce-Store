using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StricklandPropane.Data;
using StricklandPropane.Models;

namespace StricklandPropane.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _dbContext;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Produces a list of claims that are given to every user based on information
        /// collected by the user during the registration process
        /// </summary>
        /// <param name="user">The user to generate default claims for</param>
        /// <returns>A list of default claims based on the information collected on the
        /// user during the registration process</returns>
        public List<Claim> GetDefaultClaimsListForUser(ApplicationUser user) => new List<Claim>
        {
            new Claim(ClaimTypes.Name, $"{user.LastName}, {user.FirstName}", ClaimValueTypes.String),
            new Claim(ClaimTypes.Email, user.NormalizedEmail, ClaimValueTypes.Email),
            new Claim(ClaimTypes.StateOrProvince, ((int)user.HomeState).ToString(), ClaimValueTypes.Integer32),
            new Claim("GrillingPreference", ((int)user.GrillingPreference).ToString(), ClaimValueTypes.Integer32)
        };

        /// <summary>
        /// Returns the login view. If the user was already logged in, logs the user out before
        /// returning the login view.
        /// </summary>
        /// <returns>ViewResult for the login view.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return View(new LoginViewModel());
        }

        /// <summary>
        /// Attempts to log the user in using the local account credentials
        /// provided by the user.
        /// </summary>
        /// <param name="vm">View model from the login view with the user's e-mail address and password</param>
        /// <returns>RedirectToActionResult to the product dashboard if the user successfully logs in
        /// and is in the admin role. Redirects to the shop if the user is not in the admin role.
        /// If the login is not successful or the input provided did not meet validation requirements,
        /// then the login view is presented to the user to try again.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(vm.Email,
                    vm.Password, vm.KeepSignedIn, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    ApplicationUser user = await _userManager.FindByEmailAsync(vm.Email);

                    // If the user is an administrator, take them to the product administration
                    // dashboard; otherwise, take the user to the products landing page
                    if (await _userManager.IsInRoleAsync(user, ApplicationRoles.Admin))
                    {
                        return RedirectToAction("Index", "Products");
                    }

                    return RedirectToAction("Index", "Shop");
                }
                if (result.RequiresTwoFactor)
                {
                    throw new NotImplementedException();
                }
                if (result.IsLockedOut)
                {
                    throw new NotImplementedException();
                }
            }

            // Something went wrong or input did not pass validation. Return the view so
            // the user can try again.
            return View(vm);
        }

        /// <summary>
        /// Displays a view to ask the user for additional information not provided via the
        /// user's selected OAuth provider's token if the user does not already have an account
        /// </summary>
        /// <param name="provider">The name of the OAuth provider</param>
        /// <param name="email">The e-mail address of the user collected from OAuth</param>
        /// <returns>View to collect additional information from the user</returns>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ExternalRegister(string provider, string email)
        {
            return View(new ExternalRegisterViewModel()
            {
                Provider = provider,
                Email = email
            });
        }

        /// <summary>
        /// Creates the user account for users who do not already have an account and have
        /// been successfully authenticated via OAuth
        /// </summary>
        /// <param name="ervm">The completed ExternalRegisterViewModel containing the user's
        /// answers to the profile questions that were not able to be populated via the OAuth
        /// token.</param>
        /// <returns>Redirects to the product dashboard if the user is in the admin role.
        /// Redirects to the shop if the user is not an admin. If input validation failed
        /// then re-display the external registration view so the user can try again.</returns>
        [HttpPost]
        [AllowAnonymous]
        [ActionName("ExternalRegister")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CommitExternalRegister(ExternalRegisterViewModel ervm)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser()
                {
                    Email = ervm.Email,
                    NormalizedEmail = ervm.Email.ToLower(),
                    UserName = ervm.Email,
                    NormalizedUserName = ervm.Email.ToLower(),
                    EmailConfirmed = true, // Skip e-mail verification for external logins

                    FirstName = ervm.FirstName,
                    LastName = ervm.LastName,
                    GrillingPreference = ervm.GrillingPreference,
                    HomeState = ervm.HomeState,
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                };

                // Try to create the new user (with no password)
                IdentityResult result = await _userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    // Add default claims and member role to the new user
                    await _userManager.AddClaimsAsync(user, GetDefaultClaimsListForUser(user));
                    await _userManager.AddToRoleAsync(user, ApplicationRoles.Member);

                    // Sign the user in and redirect them back to whence they came
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    // Try to get the user's external login provider information
                    var info = await _signInManager.GetExternalLoginInfoAsync();

                    // Could not get the external login info
                    if (info is null)
                    {
                        return RedirectToAction(nameof(Login));
                    }

                    // Add the user's external login provider
                    await _userManager.AddLoginAsync(user, info);

                    // If the user is an administrator, take them to the product administration
                    // dashboard; otherwise, take the user to the products landing page
                    if (await _userManager.IsInRoleAsync(user, ApplicationRoles.Admin))
                    {
                        return RedirectToAction("Index", "Products");
                    }

                    return RedirectToAction("Index", "Shop");
                }

                // Something went wrong. Accumulate all errors into the model state.
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Something went wrong. Allow the user to try again
            return View(ervm);
        }

        /// <summary>
        /// Initiates an OAuth challenge using the user's selected OAuth provider
        /// </summary>
        /// <param name="provider">The provider that the user would like to use to
        /// authenticate with via OAuth</param>
        /// <returns>ChallengeResult using the Identity authentication middleware</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider)
        {
            string redirectUrl = Url.Action(nameof(ExternalLoginCallbackAsync), "Account");
            AuthenticationProperties properties = _signInManager.ConfigureExternalAuthenticationProperties(
                provider, redirectUrl);

            return Challenge(properties, provider);
        }

        /// <summary>
        /// Handle the callback for the external OAuth provider's auth response
        /// </summary>
        /// <param name="remoteError">Contains any errors from the OAuth provider</param>
        /// <returns>If the user already has an account associated with the selected
        /// OAuth provider, simply logs that user in. If the user has an account already
        /// but has not used OAuth with the selected provider, then associate the provider
        /// with their account. Otherwise, redirect to ExternalRegister to collect
        /// some additional information</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallbackAsync(string remoteError = null)
        {
            if (remoteError != null)
            {
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info is null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,
                isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Shop");
            }

            string externalPrincipalEmail = info.Principal.FindFirstValue(ClaimTypes.Email);
            ApplicationUser user = await _userManager.FindByEmailAsync(externalPrincipalEmail);

            // If the user does not yet exist, redirect to the ExternalRegister action to
            // get roles, claims, and some additional information added to the external account
            if (user is null)
            {
                return RedirectToAction("ExternalRegister", new { provider = info.LoginProvider, email = externalPrincipalEmail });
            }

            // If the user already exists but has never used the selected OAuth provider to log in,
            // then add this provider to the user's account, sign in, and redirect to the shop
            if ((await _userManager.AddLoginAsync(user, info)).Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Shop");
            }

            // Something went wrong. Redirect to the login page.
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            // TODO(taylorjoshuaw): Add logging here
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ActionName("Register")]
        public async Task<IActionResult> RegisterCommit(
            [Bind("Email", "Password", "ConfirmPassword", "FirstName", "LastName", "HomeState", "GrillingPreference")] RegisterViewModel vm)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser()
                {
                    Email = vm.Email,
                    NormalizedEmail = vm.Email.ToLower(),
                    UserName = vm.Email,
                    NormalizedUserName = vm.Email.ToLower(),
                    EmailConfirmed = false,
                    ConcurrencyStamp = Guid.NewGuid().ToString(),

                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    GrillingPreference = vm.GrillingPreference,
                    HomeState = vm.HomeState
                };

                IdentityResult result = await _userManager.CreateAsync(user, vm.Password);

                if (result.Succeeded)
                {
                    // Add default claims and member role to the new user
                    await _userManager.AddClaimsAsync(user, GetDefaultClaimsListForUser(user));
                    await _userManager.AddToRoleAsync(user, ApplicationRoles.Member);

                    return RedirectToAction("Verify", "Email", new { email = user.Email });
                }

                // Something went wrong. Accumulate all errors into the model state.
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(vm);
        }

        public async Task<IActionResult> Profile()
        {
            return View(await _userManager.GetUserAsync(HttpContext.User));
        }
    }
}