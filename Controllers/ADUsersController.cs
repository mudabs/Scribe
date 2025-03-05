using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Scribe.Models;
using System.DirectoryServices.AccountManagement;
using Scribe.ViewComponents;
using Scribe.Services;
using Scribe.Data;

namespace Scribe.Controllers
{
    public class ADUsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAllocationService _allocationService;
        private readonly IDeallocationService _deallocationService;
        private readonly ILoggingService _loggingService;
        private readonly IActiveDirectoryService _adService;


        public ADUsersController(ApplicationDbContext context, IAllocationService allocationService, IDeallocationService deallocationService, ILoggingService loggingService, IActiveDirectoryService adService)
        {
            _context = context;
            _allocationService = allocationService;
            _deallocationService = deallocationService;
            _loggingService = loggingService;
            _adService = adService;
        }

        // GET: ADUsers
        public async Task<IActionResult> Index()
        {
            return View(await _context.ADUsers.ToListAsync());
        }

        //public async Task<IActionResult> Dets()
        //{
        //    // Fetch all users and their assigned devices
        //    var usersWithDevices = _context.ADUsers
        //        .Select(u => new UserDevicesViewModel
        //        {
        //            User = u,
        //            Devices = _context.SerialNumbers.Include(s => s.Model.Brand).Where(s => s.ADUsersId == u.Id).ToList()
        //        })
        //        .ToList();

        //    return View(usersWithDevices);
        //}

        // GET: ADUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aDUsers = await _context.ADUsers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aDUsers == null)
            {
                return NotFound();
            }

            return View(aDUsers);
        }

        // GET: ADUsers/Create
        public IActionResult Create()
        {
            return View();
        }
        // GET: ADUsers/Create
        public IActionResult RefreshDC()
        {
            ImportADUsersToDatabase();
            // Create a log entry using logging service
            var details = $"Domain Controller Names Updated.";
            var myUser = User.Identity.Name ?? "Anonymous"; // Assuming you have user authentication
            _loggingService.LogActionAsync(details, myUser); // Log the action
            return RedirectToAction(nameof(Index));
        }
        public IActionResult RefreshUsersDC()
        {
            _adService.StoreUsersInGroup();
            // Create a log entry using logging service
            var details = $"System Users Updated.";
            var myUser = User.Identity.Name ?? "Anonymous"; // Assuming you have user authentication
            _loggingService.LogActionAsync(details, myUser); // Log the action
            return RedirectToAction(nameof(Index));
        }

        // POST: ADUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] ADUsers aDUsers)
        {
            if (ModelState.IsValid)
            {
                _context.Add(aDUsers);
                await _context.SaveChangesAsync();

                // Create a log entry using logging service
                var details = $"New employee: '{aDUsers.Name}' added.";
                var myUser = User.Identity.Name ?? "Anonymous"; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action
                return RedirectToAction(nameof(Index));
            }
            return View(aDUsers);
        }

        // GET: ADUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aDUsers = await _context.ADUsers.FindAsync(id);
            if (aDUsers == null)
            {
                return NotFound();
            }
            return View(aDUsers);
        }

        // POST: ADUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] ADUsers aDUsers)
        {
            if (id != aDUsers.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aDUsers);
                    await _context.SaveChangesAsync();

                    // Create a log entry using logging service
                    var details = $"User: '{aDUsers.Name}' details updated.";
                    var myUser = User.Identity.Name ?? "Anonymous"; // Assuming you have user authentication
                    await _loggingService.LogActionAsync(details, myUser); // Log the action
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ADUsersExists(aDUsers.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(aDUsers);
        }

        // GET: ADUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aDUsers = await _context.ADUsers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (aDUsers == null || id == 1)
            {
                TempData["Failure"] = "The User Cannot be Deleted";
                return RedirectToAction(nameof(Index));
            }

            return View(aDUsers);
        }

        // POST: ADUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aDUsers = await _context.ADUsers.FindAsync(id);
            if (aDUsers != null || aDUsers.Name != "No User" || id != 1)
            {
                // Create a log entry using logging service
                var details = $"User '{aDUsers.Name}' deleted. ";
                var myUser = User.Identity.Name ?? "Anonymous"; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action

                _context.ADUsers.Remove(aDUsers);
            }
            else
            {
                TempData["Failure"] = "User Cannot be Deleted";
            }

            var serialNumbers = _context.SerialNumbers.Where(sn => sn.ADUsersId == null).ToList();
            foreach (var serialNumber in serialNumbers)
            {
                serialNumber.ADUsersId = 1; // Set to 'No User' (ID 1)
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "User Deleted Successfully";
            return RedirectToAction(nameof(Index));
        }

        private bool ADUsersExists(int id)
        {
            return _context.ADUsers.Any(e => e.Id == id);
        }

        public void ImportADUsersToDatabase()
        {
            // Create a list to hold the users from Active Directory
            List<ADUsers> adUsers = new List<ADUsers>();

            // Instantiate Active Directory Server Connection
            using (PrincipalContext adServer = new PrincipalContext(ContextType.Domain, null)) // default domain
            {
                var userPrincipal = new UserPrincipal(adServer);
                using (var search = new PrincipalSearcher(userPrincipal))
                {
                    foreach (var p in search.FindAll())
                    {
                        if (p.DisplayName != null)
                        {
                            // Create a new ADUsers instance
                            var newUser = new ADUsers { Name = p.DisplayName };

                            // Check if the user already exists in the database using a case-insensitive comparison
                            bool userExists = _context.ADUsers
                                .Any(u => u.Name.ToLower() == newUser.Name.ToLower()); // Use ToLower for case-insensitive comparison

                            // Add the user to the list if they do not exist
                            if (!userExists)
                            {
                                adUsers.Add(newUser);
                            }
                        }
                    }
                }
            }

            // Add new users to the database
            if (adUsers.Count > 0)
            {
                _context.ADUsers.AddRange(adUsers);
                _context.SaveChanges(); // Commit the changes to the database

            }
            TempData["Success"] = "Importing Users From DC Complete";
        }


        //Individual Allocations Logic
        //Allocating a device to a user
        public async Task<IActionResult> AllocateUser(int id)
        {
            // Prepare data for the view
            var users = await _context.ADUsers.ToListAsync();
            var brands = await _context.Brands.ToListAsync();

            ViewData["UserId"] = new SelectList(users, "Id", "Name");
            ViewData["BrandId"] = new SelectList(brands, "Id", "Name");

            // Fetch the group based on the provided id
            var user = await _context.ADUsers.FindAsync(id); // Assuming Groups is your DbSet for the Group model

            // Check if the group exists
            if (user == null)
            {
                return NotFound(); // Handle case where the group does not exist
            }

            var viewModel = new IndividualAssignmentViewModel
            {
                ADUsersId = id,

            };

            var serialNumberIds = await _context.SerialNumberGroup
                .Where(u => u.ADUsersId == id)
                .Include(s => s.SerialNumber.Model)
                .Include(s => s.SerialNumber.Model.Brand)
                .Include(s => s.SerialNumber.Model.Category)
                .Select(s => s.SerialNumberId)
                .ToListAsync();

            viewModel.SerialNumbers = await _context.SerialNumberGroup
                .Where(u => serialNumberIds.Contains(u.SerialNumberId))
                .Include(s => s.SerialNumber.Model)
                .Include(s => s.SerialNumber.Model.Brand)
                .Include(s => s.SerialNumber.Model.Category)
                .ToListAsync();


            // Pass the group model to ViewData
            ViewData["User"] = user;
            ViewData["CurrentDateTime"] = DateTime.Now;

            // Pass the id to the ViewComponent or directly to the view
            return ViewComponent("IndividualAssignment", new { id, model = viewModel });
        }

        //Cascading Dropdown
        [HttpGet]
        public JsonResult GetModelsByBrand(int brandId)
        {
            var models = _context.Models.Where(m => m.BrandId == brandId)
                                         .Select(m => new { Id = m.Id, Name = m.Name })
                                         .ToList();
            return Json(models);
        }

        [HttpGet]
        public JsonResult GetSerialNumbersByModel(int modelId)
        {
            var serialNumbers = _context.SerialNumbers.Where(s => s.ModelId == modelId)
                                                      .Select(s => new { Id = s.Id, Name = s.Name })
                                                      .ToList();
            return Json(serialNumbers);
        }

        [HttpPost]
        public async Task<IActionResult> CreateIndividualAllocation(int ADUsersId, int SerialNumberId, string allocatedBy)
        {
            
            try
            {
                if(allocatedBy == null)
                {
                    allocatedBy = User.Identity.Name;
                }
                await _allocationService.CreateAllocationAsync(SerialNumberId, ADUsersId, DateTime.Now, null, allocatedBy);
                TempData["Success"] = "Allocation Log Added";
            }
            catch (Exception ex)
            {
                TempData["Failure"] = ex.Message; // Handle the exception as needed
            }

            return RedirectToAction("AllocateUser", new { id = ADUsersId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveDevice(int ADUsersId, int SerialNumberId, string deallocatedBy)
        {
            // Validate the input
            if (ADUsersId == 0)
            {
                TempData["Failure"] = "Allocation not found.";
                return RedirectToAction("AllocateUser", new { id = ADUsersId });
            }

            // Find the allocation to delete
            var allocationToDelete = await _context.SerialNumberGroup
        .FirstOrDefaultAsync(sng => sng.ADUsersId == ADUsersId && sng.SerialNumberId == SerialNumberId);
            var allocationHistoryId = await _context.AllocationHistory
    .Where(x => x.ADUsersId == ADUsersId && x.SerialNumberId == SerialNumberId)
    .OrderByDescending(x => x.AllocationDate) // Sort by CreationDate in descending order
    .FirstOrDefaultAsync(); // Get the most recent record

            if (allocationToDelete != null)
            {
                try
                {
                    if(deallocatedBy == null)
                    {
                        deallocatedBy = User.Identity.Name;
                    }
                    //USING THE SERVICE
                    await _deallocationService.DeallocateAsync(allocationHistoryId.Id, deallocatedBy);
                    TempData["Success"] = "Device deallocated successfully.";
                }
                catch (InvalidOperationException ex)
                {
                    TempData["Failure"] = ex.Message;
                }
            }
            else
            {
                TempData["Failure"] = "Allocation Not Available";
            }


            TempData["Success"] = "Allocation has been deleted successfully.";
            return RedirectToAction("AllocateUser", new { id = ADUsersId });
        }

    }
}
