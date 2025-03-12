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
        Task CreateAllocationAsync(int serialNumberId, int adUsersId, DateTime allocationDate, DateTime? deallocationDate, string allocatedBy);
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

        public async Task CreateAllocationAsync(int serialNumberId, int adUsersId, DateTime allocationDate, DateTime? deallocationDate, string allocatedBy)
        {
            //Checking existence of current allocation
            var sng = await _context.SerialNumberGroup.FirstOrDefaultAsync(x => x.SerialNumberId == serialNumberId && x.ADUsersId == adUsersId);
            if (sng == null)
            {
                //// Check if the SerialNumber is currently allocated another person
                //var existingAllocation = await _context.SerialNumberGroup
                //    .FirstOrDefaultAsync(sng => sng.SerialNumberId == serialNumberId);

                //if (existingAllocation != null)
                //{
                //    //will change logic to add notification
                //    _context.Remove(existingAllocation);
                //}

                // Creating an Allocation History
                AllocationHistory allocationHistory = new AllocationHistory()
                {
                    SerialNumberId = serialNumberId,
                    ADUsersId = adUsersId,
                    AllocationDate = allocationDate,
                    DeallocationDate = deallocationDate,
                    AllocatedBy = allocatedBy
                };

                //Updating Serial Number User and Condition
                var sn = await _context.SerialNumbers.Include(x => x.ADUsers).Include(x => x.Model).Include(x => x.Model.Brand).Include(x => x.Model.Category).FirstOrDefaultAsync(x => x.Id == serialNumberId);
                sn.ADUsersId = adUsersId;
                //Find Condition with "In Use"
                var condId = _context.Condition.FirstOrDefault(x => x.Name == "In Use").Id;
                sn.ConditionId = condId;
                sn.AllocatedBy = allocatedBy;

                //Creating SerialNumber Group
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

                // Create a log entry using logging service
                var employee = await _context.ADUsers.FindAsync(sn.Id);
                var details = $"Serial Number '{sn.Name}' for '{sn.Model.Brand.Name}' '{sn.Model.Name}' '{sn.Model.Category.Name}' allocated to '{employee.Name}'.";
                var myUser = GetCurrentUserName(); // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action
            }
            else
            {
                throw new InvalidOperationException("User is Currently Allocated this Device.");
            }


        }

    }
}
