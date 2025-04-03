using Scribe.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Scribe.Models;
using Scribe.Data;



namespace Scribe.Services
{

    public interface IDeallocationService
    {
        Task DeallocateAsync(int allocationId, string deallocatedBy);
    }
    public class DeallocationService : IDeallocationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILoggingService _loggingService;

        public DeallocationService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, ILoggingService loggingService)
        {
            _context = context;
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


        public async Task DeallocateAsync(int allocationId, string deallocatedBy)
        {
            // Find the allocation history entry by ID
            var allocationHistory = await _context.AllocationHistory.FindAsync(allocationId);
            if (allocationHistory == null)
            {
                throw new InvalidOperationException("Allocation Log not found.");
            }

            var currentSerialNumber = await _context.SerialNumbers.Include(x=>x.Model).Include(x=>x.Model.Brand).Include(x=>x.Model.Category).Include(x=>x.Group).Include(x=>x.ADUsers).FirstOrDefaultAsync(x=>x.Id == allocationHistory.SerialNumberId);
            var serialNumberGroup = await _context.SerialNumberGroup.FirstOrDefaultAsync(x => x.SerialNumberId == allocationHistory.SerialNumberId);

            var employee = await _context.ADUsers.FindAsync(currentSerialNumber.ADUsersId);
            var brand = await _context.Brands.FirstOrDefaultAsync(x => x.Id == currentSerialNumber.Model.BrandId);
            var model = await _context.Models.FirstOrDefaultAsync(x => x.Id == currentSerialNumber.ModelId);
            var category = await _context.Categories.FirstOrDefaultAsync(x => x.Id == currentSerialNumber.Model.CategoryId);

            // Remove the allocation history entry
            allocationHistory.DeallocationDate = DateTime.Now;
            allocationHistory.DeallocatedBy = deallocatedBy;
            _context.AllocationHistory.Update(allocationHistory);
            

            // Set User to No User in SerialNumbers Table
            if (currentSerialNumber != null)
            {
                var details = "det";
                if (currentSerialNumber.Group != null)
                {
                    details = $"Serial Number '{currentSerialNumber.Name}' for '{brand.Name}' '{model.Name}' '{category.Name}' deallocated from '{currentSerialNumber.Group.Name}'.";
                }
                else if (currentSerialNumber.ADUsers != null)
                {
                    details = $"Serial Number '{currentSerialNumber.Name}' for '{brand.Name}' '{model.Name}' '{category.Name}' deallocated from '{employee.Name}'.";
                }

                // Create a log entry using logging service
                var myUser = GetCurrentUserName(); // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action


                var aduserNone = _context.ADUsers.FirstOrDefault(x => x.Name == "No User").Id; // Looking for Id of "No User"
                var condId = _context.Condition.FirstOrDefault(x => x.Name == "Awaiting User").Id; // Looking for Id of "No User"
                currentSerialNumber.ADUsersId = aduserNone; 
                currentSerialNumber.ConditionId = condId; 
                currentSerialNumber.DeallocatedBy = deallocatedBy; 
                currentSerialNumber.CurrentlyAllocated = false; 
                _context.SerialNumbers.Update(currentSerialNumber);
            }



            // Remove Device from SerialNumberGroup Table
            if (serialNumberGroup != null)
            {
                _context.SerialNumberGroup.Remove(serialNumberGroup);
            }

            await _context.SaveChangesAsync();
        }
    }
}
