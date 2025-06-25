using Scribe.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Scribe.Models;
using Scribe.Data;



namespace Scribe.Services
{
    public interface IAllocationService
    {
        Task CreateAllocationAsync(int serialNumberId, int adUsersId, DateTime? allocationDate, DateTime? deallocationDate, string allocatedBy);
        Task<bool> AllocationExists(int serialNumberId, int adUsersId, DateTime allocationDate, DateTime? deallocationDate, string allocatedBy);
        Task<bool> MyAllocationExists(int serialNumberId, int adUsersId, DateTime allocationDate, DateTime? deallocationDate, string allocatedBy);
          }
    public class AllocationService : IAllocationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILoggingService _loggingService;

        public AllocationService(ApplicationDbContext context, IHttpContextAccessor contextAccessor, ILoggingService loggingService)
        {
            _context = context;
            _contextAccessor = contextAccessor;
            _loggingService = loggingService;
        }

        public string GetCurrentUserName()
        {
            var httpContext = _contextAccessor.HttpContext;

            if (httpContext != null && httpContext.User.Identity.IsAuthenticated)
            {
                return httpContext.User.Identity.Name; // Access User from HttpContext
            }

            return "Anonymous"; // Handle unauthenticated users
        }
        public async Task<bool> AllocationExists (int serialNumberId, int adUsersId, DateTime allocationDate, DateTime? deallocationDate, string allocatedBy)
        {
            //Checking if the user is allocated this Device
            var sng = await _context.SerialNumberGroup.FirstOrDefaultAsync(x => x.SerialNumberId == serialNumberId);
            if (sng == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }   
        public async Task<bool> MyAllocationExists (int serialNumberId, int adUsersId, DateTime allocationDate, DateTime? deallocationDate, string allocatedBy)
        {
            //Checking if the user is allocated this Device
            var sng = await _context.SerialNumberGroup.FirstOrDefaultAsync(x => x.SerialNumberId == serialNumberId && x.ADUsersId == adUsersId);
            if (sng == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task CreateAllocationAsync(int serialNumberId, int adUsersId, DateTime? allocationDate, DateTime? deallocationDate, string allocatedBy)
        {
            // Use today's date if allocationDate is null
            var effectiveAllocationDate = allocationDate ?? DateTime.Now;

            // Checking existence of current allocation
            var sng = await _context.SerialNumberGroup.FirstOrDefaultAsync(x => x.SerialNumberId == serialNumberId && x.ADUsersId == adUsersId);
            if (sng == null)
            {
                // Creating an Allocation History
                AllocationHistory allocationHistory = new AllocationHistory()
                {
                    SerialNumberId = serialNumberId,
                    ADUsersId = adUsersId,
                    AllocationDate = effectiveAllocationDate,
                    DeallocationDate = deallocationDate,
                    AllocatedBy = allocatedBy
                };

                // Updating Serial Number User and Condition
                var sn = await _context.SerialNumbers
                    .Include(x => x.ADUsers)
                    .Include(x => x.Model).ThenInclude(m => m.Brand)
                    .Include(x => x.Model).ThenInclude(m => m.Category)
                    .Include(x => x.Location)
                    .FirstOrDefaultAsync(x => x.Id == serialNumberId);

                sn.ADUsersId = adUsersId;
                sn.ConditionId = _context.Condition.FirstOrDefault(x => x.Name == "In Use").Id;
                sn.LocationId = _context.Locations.FirstOrDefault(x => x.Name == "User Station").Id;
                sn.AllocatedBy = allocatedBy;
                sn.Allocation = effectiveAllocationDate;
                sn.CurrentlyAllocated = true;
                sn.GroupId = null;

                // Creating SerialNumber Group
                SerialNumberGroup serialNumberGroup = new SerialNumberGroup
                {
                    ADUsersId = adUsersId,
                    SerialNumberId = serialNumberId,
                    GroupId = null
                };

                await _context.AllocationHistory.AddAsync(allocationHistory);
                await _context.SerialNumberGroup.AddAsync(serialNumberGroup);
                _context.SerialNumbers.Update(sn);
                await _context.SaveChangesAsync();

                // Logging
                var employee = await _context.ADUsers.FindAsync(adUsersId);
                var details = $"Serial Number '{sn.Name}' for '{sn.Model.Brand.Name}' '{sn.Model.Name}' '{sn.Model.Category.Name}' allocated to '{employee.Name}'.";
                var myUser = GetCurrentUserName();
                await _loggingService.LogActionAsync(details, myUser);
            }
            else
            {
                throw new InvalidOperationException("User is Currently Allocated this Device.");
            }
        }
    }
}
