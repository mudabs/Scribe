using Microsoft.AspNetCore.Identity;
using Scribe.Models;
using Microsoft.EntityFrameworkCore;
using Scribe.Infrastructure;
using System.Data;

namespace Scribe.Services
{
    // IUserService.cs
    public interface IUserService
    {
        string GetCurrentUserName();
        Task LogActionAsync(string details);
    }


    // UserService.cs
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILoggingService _loggingService;

        public UserService(IHttpContextAccessor httpContextAccessor, ILoggingService loggingService)
        {
            _httpContextAccessor = httpContextAccessor;
            _loggingService = loggingService;
        }

        public string GetCurrentUserName()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null && httpContext.User.Identity.IsAuthenticated)
            {
                return httpContext.User.Identity.Name; // Access User from HttpContext
            }

            return "Anonymous"; // Handle unauthenticated users
        }

        // Other methods that do not rely on UserManager or RoleManager
        // For example, logging actions or retrieving user information from AD

        public async Task LogActionAsync(string details)
        {
            var myUser = GetCurrentUserName(); // Assuming you have user authentication
            await _loggingService.LogActionAsync(details, myUser); // Log the action
        }
    }
}

