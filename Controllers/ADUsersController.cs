using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Scribe.Models;
using System.DirectoryServices.AccountManagement;
using Scribe.Services;
using Scribe.Infrastructure;

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
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "AD Users", Url = Url.Action("Index", "ADUsers"), IsActive = true }
        };

            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(await _context.ADUsers.ToListAsync());
        }

        // GET: ADUsers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "AD Users", Url = Url.Action("Index", "ADUsers"), IsActive = false },
            new BreadcrumbItem { Title = "Details", Url = Url.Action("Details", "ADUsers", new { id }), IsActive = true }
        };

            ViewData["Breadcrumbs"] = breadcrumbs;

            if (id == null)
            {
                return NotFound();
            }

            var aDUsers = await _context.ADUsers.FirstOrDefaultAsync(m => m.Id == id);
            if (aDUsers == null)
            {
                return NotFound();
            }

            return View(aDUsers);
        }

        // GET: ADUsers/Create
        public IActionResult Create()
        {
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "AD Users", Url = Url.Action("Index", "ADUsers"), IsActive = false },
            new BreadcrumbItem { Title = "Create", Url = Url.Action("Create", "ADUsers"), IsActive = true }
        };

            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("_Create");
        }

        // GET: ADUsers/RefreshDC
        public IActionResult RefreshDC()
        {
            ImportADUsersToDatabase();
            // Create a log entry using logging service
            var details = $"Domain Controller Names Updated.";
            var myUser = User.Identity.Name ?? "Anonymous"; // Assuming you have user authentication
            _loggingService.LogActionAsync(details, myUser); // Log the action
            return RedirectToAction(nameof(Index));
        }

        // GET: ADUsers/RefreshUsersDC
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] ADUsers aDUsers)
        {
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "AD Users", Url = Url.Action("Index", "ADUsers"), IsActive = false },
            new BreadcrumbItem { Title = "Create", Url = Url.Action("Create", "ADUsers"), IsActive = true }
        };

            ViewData["Breadcrumbs"] = breadcrumbs;

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
            return PartialView("_Create");
        }

        // GET: ADUsers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "AD Users", Url = Url.Action("Index", "ADUsers"), IsActive = false },
            new BreadcrumbItem { Title = "Edit", Url = Url.Action("Edit", "ADUsers", new { id }), IsActive = true }
        };

            ViewData["Breadcrumbs"] = breadcrumbs;

            if (id == null)
            {
                return NotFound();
            }

            var aDUsers = await _context.ADUsers.FindAsync(id);
            if (aDUsers == null)
            {
                return NotFound();
            }
            return PartialView("_Edit", aDUsers);
        }

        // POST: ADUsers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] ADUsers aDUsers)
        {
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "AD Users", Url = Url.Action("Index", "ADUsers"), IsActive = false },
            new BreadcrumbItem { Title = "Edit", Url = Url.Action("Edit", "ADUsers", new { id }), IsActive = true }
        };

            ViewData["Breadcrumbs"] = breadcrumbs;

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
            return PartialView("_Edit", aDUsers);
        }

        // GET: ADUsers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "AD Users", Url = Url.Action("Index", "ADUsers"), IsActive = false },
            new BreadcrumbItem { Title = "Delete", Url = Url.Action("Delete", "ADUsers", new { id }), IsActive = true }
        };

            ViewData["Breadcrumbs"] = breadcrumbs;

            if (id == null)
            {
                return NotFound();
            }

            var aDUsers = await _context.ADUsers.FirstOrDefaultAsync(m => m.Id == id);
            if (aDUsers == null || id == 1)
            {
                TempData["Failure"] = "The User Cannot be Deleted";
                return RedirectToAction(nameof(Index));
            }

            return PartialView("_Delete", aDUsers);
        }



        // POST: ADUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aDUsers = await _context.ADUsers.FindAsync(id);
            if (aDUsers != null && aDUsers.Name != "No User" && id != 1)
            {
                // Check if the ADUserId is present in the SerialNumberGroup table
                bool isUserInSerialNumberGroup = await _context.SerialNumberGroup.AnyAsync(sng => sng.ADUsersId == id);
                if (isUserInSerialNumberGroup)
                {
                    TempData["Failure"] = "User cannot be deleted because they are associated with a serial number.";
                    return RedirectToAction("Index", "ADUsers");
                }

                // Create a log entry using logging service
                var details = $"User '{aDUsers.Name}' deleted.";
                var myUser = User.Identity.Name ?? "Anonymous"; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action

                _context.ADUsers.Remove(aDUsers);

                var serialNumbers = _context.SerialNumbers.Where(sn => sn.ADUsersId == null).ToList();
                foreach (var serialNumber in serialNumbers)
                {
                    serialNumber.ADUsersId = 1; // Set to 'No User' (ID 1)
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "User deleted successfully.";
            }
            else
            {
                TempData["Failure"] = "User cannot be deleted.";
            }

            return RedirectToAction("Index", "ADUsers");
        }
        private bool ADUsersExists(int id)
        {
            return _context.ADUsers.Any(e => e.Id == id);
        }

        public void ImportADUsersToDatabase()
        {
            string domain = "zlt.co.zw";
            string ouPath = "OU=Domain Users,DC=zlt,DC=co,DC=zw";

            // Create a list to hold the users from Active Directory
            List<ADUsers> adUsers = new List<ADUsers>();

            // Instantiate Active Directory Server Connection
            using (PrincipalContext adServer = new PrincipalContext(ContextType.Domain, domain, ouPath))
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
                                .Any(u => u.Name.ToLower() == newUser.Name.ToLower());

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

        // Individual Allocations Logic
        // Allocating a device to a user
        public async Task<IActionResult> AllocateUser(int id)
        {
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "AD Users", Url = Url.Action("Index", "ADUsers"), IsActive = false },
            new BreadcrumbItem { Title = "Allocate User", Url = Url.Action("AllocateUser", "ADUsers", new { id }), IsActive = true }
        };

            ViewData["Breadcrumbs"] = breadcrumbs;

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
            var serialNumbers = _context.SerialNumbers
                                        .Where(s => s.ModelId == modelId && !_context.SerialNumberGroup.Any(g => g.SerialNumberId == s.Id))
                                        .Select(s => new { Id = s.Id, Name = s.Name })
                                        .ToList();

            return Json(serialNumbers);
        }

        [HttpPost]
        public async Task<IActionResult> CreateIndividualAllocation(int ADUsersId, int SerialNumberId, string allocatedBy)
        {
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "AD Users", Url = Url.Action("Index", "ADUsers"), IsActive = false },
            new BreadcrumbItem { Title = "Allocate User", Url = Url.Action("AllocateUser", "ADUsers", new { id = ADUsersId }), IsActive = true }
        };

            ViewData["Breadcrumbs"] = breadcrumbs;

            try
            {
                if (allocatedBy == null)
                {
                    allocatedBy = User.Identity.Name;
                }
                var result = await _allocationService.AllocationExists(SerialNumberId, ADUsersId, DateTime.Now, null, allocatedBy);
                var doIExist = await _allocationService.MyAllocationExists(SerialNumberId, ADUsersId, DateTime.Now, null, allocatedBy);

                // Checking if I have already been allocated
                if (doIExist)
                {
                    var model = await _context.SerialNumberGroup.FirstOrDefaultAsync(x => x.SerialNumberId == SerialNumberId);
                    TempData["Failure"] = "Device is already allocated to user.";
                    return Json(new { success = false, message = "Device is already allocated to user." });
                }

                // Checking if device is already allocated to someone else
                if (result)
                {
                    var model = await _context.SerialNumberGroup.Include(x => x.ADUsers).Include(x => x.SerialNumber).FirstOrDefaultAsync(x => x.SerialNumberId == SerialNumberId);
                    if (model.GroupId != null)
                    {
                        return PartialView("_ConfirmRemovalFromGroup", model);
                    }
                    return PartialView("_ConfirmRemoval", model);
                }
                else
                {
                    await _allocationService.CreateAllocationAsync(SerialNumberId, ADUsersId, DateTime.Now, null, allocatedBy);
                    TempData["Success"] = "Allocation Log Added";
                    return Json(new { success = true, message = "Allocation Log Added" });
                }

            }
            catch (Exception ex)
            {
                TempData["Failure"] = ex.Message; // Handle the exception as needed
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public async Task<IActionResult> RemoveDevice(int ADUsersId, int SerialNumberId, string deallocatedBy)
        {
            var breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                    new BreadcrumbItem { Title = "AD Users", Url = Url.Action("Index", "ADUsers"), IsActive = false },
                    new BreadcrumbItem { Title = "Allocate User", Url = Url.Action("AllocateUser", "ADUsers", new { id = ADUsersId }), IsActive = true }
                };

            ViewData["Breadcrumbs"] = breadcrumbs;


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
                    if (deallocatedBy == null)
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
