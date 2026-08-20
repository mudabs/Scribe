using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Scribe.Services;
using Scribe.Models;

namespace Scribe.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly ILoggingService _loggingService;
        private readonly IConfiguration _configuration;

        public AccountController(ILoggingService loggingService, IConfiguration configuration)
        {
            _loggingService = loggingService;
            _configuration = configuration;
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
            var configuredUsername = _configuration["DemoAuthentication:Username"];
            var configuredPassword = _configuration["DemoAuthentication:Password"];

            if (!string.IsNullOrWhiteSpace(configuredUsername) &&
                !string.IsNullOrWhiteSpace(configuredPassword) &&
                string.Equals(username?.Trim(), configuredUsername.Trim(), StringComparison.Ordinal) &&
                string.Equals(password, configuredPassword, StringComparison.Ordinal))
            {
                validationMessage = string.Empty;
                return true;
            }

            validationMessage = "Invalid username or password.";
            TempData["Failure"] = validationMessage;
            return false;
        }
    }
}
