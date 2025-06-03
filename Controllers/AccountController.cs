using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.DirectoryServices.AccountManagement;
using Scribe.Services;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Humanizer;
using System.Security;
using Scribe.Models;

namespace Scribe.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly string domain = "zlt.co.zw";
        private readonly string groupName = "Scribe Admins";
        private readonly ILoggingService _loggingService;

        public AccountController(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        [HttpGet]
        public IActionResult Login()
        {

            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Login", Url = Url.Action("Login", "Account"), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (ValidateUser(username, password, out string validationMessage))
            {
                if (IsUserInGroup(username))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.Role, "Scribe Admins")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                    TempData["Success"] = "Welcome " + username;
                    var details = "User " + username + " logged in.";
                    await _loggingService.LogActionAsync(details, username);

                    Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                    Response.Headers["Pragma"] = "no-cache";
                    Response.Headers["Expires"] = "0";

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "You do not have permission to access this system.");
                    TempData["Failure"] = "You do not have permission to access this system.";
                }
            }
            else
            {
                ModelState.AddModelError("", validationMessage);
            }

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Login", Url = Url.Action("Login", "Account"), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var details = "User " + User.Identity.Name + " logged out.";
            await _loggingService.LogActionAsync(details, User.Identity.Name);
            TempData["Success"] = "Logged Out";

            Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Logout", Url = Url.Action("Logout", "Account"), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return RedirectToAction("Login", "Account");
        }

        private bool ValidateUser(string username, string password, out string validationMessage)
        {
            using (var context = new PrincipalContext(ContextType.Domain, domain))
            {
                if (context.ValidateCredentials(username, password))
                {
                    validationMessage = string.Empty;
                    return true;
                }
                else
                {
                    using (var user = UserPrincipal.FindByIdentity(context, username))
                    {
                        if (user != null && user.IsAccountLockedOut())
                        {
                            validationMessage = "Your account is locked. Please try again later.";
                            TempData["Failure"] = "Your account is locked. Please try again later.";
                        }
                        else
                        {
                            validationMessage = "Invalid username or password.";
                            TempData["Failure"] = "Invalid username or password.";
                        }
                    }
                    return false;
                }
            }
        }

        private bool IsUserInGroup(string username)
        {
            using (var context = new PrincipalContext(ContextType.Domain, domain))
            using (var user = UserPrincipal.FindByIdentity(context, username))
            using (var group = GroupPrincipal.FindByIdentity(context, groupName))
            {
                if (user != null && group != null)
                {
                    return group.GetMembers().Any(member => member.SamAccountName.Equals(username, StringComparison.OrdinalIgnoreCase));
                }
            }
            return false;
        }
    }
}