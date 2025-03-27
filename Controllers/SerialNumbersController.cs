using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Scribe.Infrastructure;
using Scribe.Models;
using Scribe.Services;
using System.Security.Claims;
using Scribe.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.Elfie.Extensions;


namespace Scribe.Controllers
{
    [Authorize(Policy = "GroupPolicy")]
    public class SerialNumbersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDeallocationService _deallocationService;
        private readonly IAllocationService _allocationService;
        private readonly ILoggingService _loggingService;
        private readonly IActiveDirectoryService _adService;

        public SerialNumbersController(ApplicationDbContext context, IDeallocationService deallocationService, IAllocationService allocationService, ILoggingService loggingService, IActiveDirectoryService adService)
        {
            _context = context;
            _deallocationService = deallocationService;
            _allocationService = allocationService;
            _loggingService = loggingService;
            _adService = adService;
        }

        // GET: SerialNumbers
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.SerialNumbers.Include(s => s.Condition).Include(s => s.Department).Include(s => s.Location).Include(s => s.Model).Include(s => s.Model.Brand).Include(s => s.ADUsers);
            var count = _context.SerialNumbers.Count();
            ViewData["count"] = count;
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name");
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Name");
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name");
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Name");

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: SerialNumbers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serialNumber = await _context.SerialNumbers
                .Include(s => s.Condition)
                .Include(s => s.Department)
                .Include(s => s.Location)
                .Include(s => s.Model)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serialNumber == null)
            {
                return NotFound();
            }

            return View(serialNumber);
        }

        // GET: SerialNumbers/Create
        public IActionResult Create()
        {
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Id");
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Id");
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name");
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Name");
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Name");
            return View();
        }

        // POST: SerialNumbers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ModelId,User,ConditionId,DepartmentId,LocationId,Creation,Allocation")] SerialNumber serialNumber)
        {
            if (ModelState.IsValid)
            {
                _context.Add(serialNumber);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Id", serialNumber.ConditionId);
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Id", serialNumber.DepartmentId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", serialNumber.LocationId);
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Name");
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Name", serialNumber.ModelId);
            return View(serialNumber);
        }

        // GET: SerialNumbers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serialNumber = await _context.SerialNumbers.FindAsync(id);
            if (serialNumber == null)
            {
                return NotFound();
            }
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name", serialNumber.ConditionId);
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Name", serialNumber.DepartmentId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", serialNumber.LocationId);
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Name");
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Name", serialNumber.ModelId);
            var model = await _context.Models.FindAsync(serialNumber.ModelId);
            var brand = await _context.Brands.FindAsync(model.BrandId);
            ViewData["Brand"] = brand.Name.ToString();
            ViewData["Users"] = new SelectList(_context.SystemUsers, "SamAccountName", "SamAccountName");

            //return View(serialNumber);
            return PartialView("_Edit", serialNumber);
        }

        // POST: SerialNumbers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SerialNumber serialNumber)
        {
            if (id != serialNumber.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the original entity from the database
                    var originalSerialNumber = await _context.SerialNumbers.FindAsync(id);
                    if (originalSerialNumber == null)
                    {
                        return NotFound();
                    }

                    // Check if ADUsersId has changed
                    if (originalSerialNumber.ADUsersId != serialNumber.ADUsersId)
                    {
                        // Reassign if already assigned
                        // Check if the SerialNumber is currently allocated another person
                        var existingAllocation = await _context.SerialNumberGroup
                            .FirstOrDefaultAsync(sng => sng.SerialNumberId == id);

                        if (existingAllocation != null)
                        {
                            //will change logic to add notification
                            _context.Remove(existingAllocation);

                            //Adding Deallocation Date
                            var myAllocationHistory = _context.AllocationHistory.OrderByDescending(a => a.AllocationDate).First(x => x.SerialNumberId == existingAllocation.SerialNumberId);

                            if (myAllocationHistory != null)
                            {
                                myAllocationHistory.DeallocationDate = DateTime.Now;
                                _context.AllocationHistory.Update(myAllocationHistory);
                            }
                        }
                        var individualId = _context.IndividualAssignment.FirstOrDefault(x => x.ADUsersId == serialNumber.ADUsersId);
                        var serialNumberGroup = new SerialNumberGroup();
                        
                        //If device hasn't been allocated yet
                        if (individualId != null)
                        {
                            if (individualId.ADUsersId != 1)
                            {
                                serialNumberGroup.SerialNumberId = serialNumber.Id;
                                serialNumberGroup.ADUsersId = individualId.ADUsersId;
                            }
                        }


                        var allocationHistory = new AllocationHistory
                        {
                            SerialNumberId = id,
                            ADUsersId = (int)serialNumber.ADUsersId,
                            AllocationDate = DateTime.Now,
                            DeallocationDate = null,
                        };
                        _context.AllocationHistory.Add(allocationHistory);
                        _context.SerialNumberGroup.Add(serialNumberGroup);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "The Device has been reallocated";
                    }

                    _context.Entry(originalSerialNumber).CurrentValues.SetValues(serialNumber);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SerialNumberExists(serialNumber.Id))
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

            // Populating ViewData for the dropdowns
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name", serialNumber.ConditionId);
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Name", serialNumber.DepartmentId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", serialNumber.LocationId);
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Name", serialNumber.ADUsersId);
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Name", serialNumber.ModelId);

            return View(serialNumber);
        }

        // GET: SerialNumbers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serialNumber = await _context.SerialNumbers
                .Include(s => s.Condition)
                .Include(s => s.Department)
                .Include(s => s.Location)
                .Include(s => s.ADUsers)
                .Include(s => s.Model)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serialNumber == null)
            {
                return NotFound();
            }

            return PartialView("_Delete", serialNumber);
        }


        // POST: SerialNumbers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serialNumber = await _context.SerialNumbers.FindAsync(id);
            if (serialNumber != null)
            {
                _context.SerialNumbers.Remove(serialNumber);
                TempData["Success"] = "Device Deleted Sucessfully!!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool SerialNumberExists(int id)
        {
            return _context.SerialNumbers.Any(e => e.Id == id);
        }

        // POST: Deallocate
        [HttpPost]
        public async Task<IActionResult> Deallocate(int allocationId, string deallocatedBy)
        {
            // Find the allocation history entry by ID
            var allocationHistory = await _context.AllocationHistory.FindAsync(allocationId);
            try
            {
                if (deallocatedBy == null)
                {
                    deallocatedBy = User.Identity.Name;
                }
                // USING THE SERVICE
                await _deallocationService.DeallocateAsync(allocationId, deallocatedBy);
                TempData["Success"] = "Device deallocated successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Failure"] = ex.Message;
            }
            return RedirectToAction("ViewHistory", new { id = allocationHistory.SerialNumberId }); // Redirect as needed
        }

        // GET: ViewHistory
        public async Task<IActionResult> ViewHistory(int id)
        {
            var users = await _context.ADUsers.ToListAsync();
            var modelName = _context.SerialNumbers.Include(s => s.Model).Include(s => s.Model.Category).Include(s => s.Model.Brand).Include(s => s.ADUsers).First(s => s.Id == id);
            var type = modelName.Model.Brand.Name + " " + modelName.Model.Name + " " + modelName.Model.Category.Name;
            ViewData["ADUsersId"] = users;
            ViewData["ModelName"] = type.ToString();
            ViewData["SerialNumber"] = modelName.Name.ToString();
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name");

            //Retrieving the condition to be displayed on the page
            ViewData["Condition"] = _context.Condition.Where(x => x.Id == modelName.ConditionId).Select(x => x.Name).FirstOrDefault();
            ViewData["ConditionColorCode"] = _context.Condition.Where(x => x.Id == modelName.ConditionId).Select(x => x.ColorCode).FirstOrDefault();

            // Get users from the AD group "ZIM-WEB-IT"
            ViewData["Users"] = _adService.GetGroupMembersSelectList("zim-web-it");

            var viewModel = new HistoryViewModel
            {
                SerialNumberId = id,
                Maintenance = _context.Maintenances
                    .Where(sh => sh.SerialNumberId == id)
                    .OrderByDescending(sh => sh.ServiceDate) // Order by date if needed
                    .ToList(),
                AllocationHistory = _context.AllocationHistory
                    .Where(ah => ah.SerialNumberId == id)
                    .Include(ah => ah.ADUsers) // Eager load ADUser
                    .OrderByDescending(ah => ah.AllocationDate) // Order by date if needed
                    .ToList()
            };

            ViewBag.SelectedConditionId = modelName.ConditionId; // Pass the selected condition
            return ViewComponent("History", new { id, model = viewModel });
        }

        // POST: CreateServiceLog
        [HttpPost]
        public async Task<IActionResult> CreateServiceLog(int SerialNumberId, string ServiceDescription, DateTime ServiceDate, DateTime NextServiceDate, int ConditionId, string SystemUserId)
        {
            if(SystemUserId == null)
            {
                SystemUserId = User.Identity.Name;
            }
            var serviceHistory = new Maintenance
            {
                SerialNumberId = SerialNumberId,
                ServiceDescription = ServiceDescription,
                ServiceDate = ServiceDate,
                NextServiceDate = NextServiceDate,
                ConditionId = ConditionId,
                SystemUserId = SystemUserId
            };

            var sn = await _context.SerialNumbers.FindAsync(SerialNumberId);
            sn.ConditionId = ConditionId;

            _context.SerialNumbers.Update(sn);
            _context.Maintenances.Add(serviceHistory);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Service Log Added";
            return RedirectToAction("ViewHistory", new { id = serviceHistory.SerialNumberId }); // Or wherever you want to redirect
        }

        // POST: RemoveService
        [HttpPost]
        public async Task<IActionResult> RemoveService(int serviceId)
        {
            // Find the service history entry by ID
            var serviceHistory = await _context.Maintenances.FindAsync(serviceId);

            if (serviceHistory == null)
            {
                TempData["Failure"] = "Service Log not found.";
                return RedirectToAction("ViewHistory", new { id = serviceHistory.SerialNumberId }); // Redirect to the appropriate view
            }

            _context.Maintenances.Remove(serviceHistory);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Service Log removed successfully.";
            return RedirectToAction("ViewHistory", new { id = serviceHistory.SerialNumberId }); // Redirect to the appropriate view
        }

        // POST: CreateAllocation
        [HttpPost]
        public async Task<IActionResult> CreateAllocation(int SerialNumberId, int ADUsersId, DateTime AllocationDate, DateTime? DeallocationDate, string? allocatedBy)
        {
            try
            {
                if (allocatedBy == null)
                {
                    allocatedBy = User.Identity.Name;
                }
                await _allocationService.CreateAllocationAsync(SerialNumberId, ADUsersId, AllocationDate, DeallocationDate, allocatedBy);
                TempData["Success"] = "Allocation Log Added";
            }
            catch (Exception ex)
            {
                TempData["Failure"] = ex.Message; // Handle the exception as needed
            }
            return RedirectToAction("ViewHistory", new { id = SerialNumberId }); // Redirect as needed
        }
    }
}
