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

        public List<Claim> GetDefaultClaimsListForUser(ApplicationUser user) => new List<Claim>
        {
            new Claim(ClaimTypes.Name, $"{user.LastName}, {user.FirstName}", ClaimValueTypes.String),
            new Claim(ClaimTypes.Email, user.NormalizedEmail, ClaimValueTypes.Email),
            new Claim(ClaimTypes.StateOrProvince, ((int)user.HomeState).ToString(), ClaimValueTypes.Integer32),
            new Claim("GrillingPreference", ((int)user.GrillingPreference).ToString(), ClaimValueTypes.Integer32)
        };

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            return View(new LoginViewModel());
        }

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

            return View(vm);
        }

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

                    /*
                    // TODO(taylorjoshuaw): Redirect to e-mail verification instead of signing in
                    // Sign the user in and redirect them back to whence they came
                    await _signInManager.SignInAsync(user, isPersistent: true);

                    // If the user is an administrator, take them to the product administration
                    // dashboard; otherwise, take the user to the products landing page
                    if (await _userManager.IsInRoleAsync(user, ApplicationRoles.Admin))
                    {
                        return RedirectToAction("Index", "Products");
                    }

                    return RedirectToAction("Index", "Shop");
                    */
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