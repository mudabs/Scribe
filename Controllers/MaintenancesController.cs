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
using System.Drawing.Drawing2D;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Scribe.Data;

namespace Scribe.Controllers
{
    public class MaintenancesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;
        private readonly IActiveDirectoryService _adService;

        public MaintenancesController(ApplicationDbContext context, ILoggingService loggingService, IActiveDirectoryService adService)
        {
            _context = context;
            _loggingService = loggingService;
            _adService = adService;
        }

        // GET: Maintenances
        public async Task<IActionResult> Index()
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Maintenance", Url = Url.Action("Index", "Maintenances"), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            var applicationDbContext = _context.Maintenances.Include(s => s.SerialNumber).Include(s => s.Condition).Include(s => s.SerialNumber.Model.Brand).Include(s => s.SerialNumber.Model);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Maintenances/Details/5
        public async Task<IActionResult> Details(int? id)
        {


            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Maintenance", Url = Url.Action("Index", "Maintenances"), IsActive = false },
                new BreadcrumbItem { Title = "Details", Url = Url.Action("Details", "Maintenances"), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;
            if (id == null)
            {
                return NotFound();
            }

            var serviceHistory = await _context.Maintenances
                .Include(s => s.SerialNumber)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serviceHistory == null)
            {
                return NotFound();
            }

            return View(serviceHistory);
        }

        // GET: Maintenances/Create
        public IActionResult Create()
        {


            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Maintenance", Url = Url.Action("Index", "Maintenances"), IsActive = false },
                new BreadcrumbItem { Title = "Create", Url = Url.Action("Create", "Maintenances"), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            ViewData["SerialNumberId"] = new SelectList(_context.SerialNumbers, "Id", "Name");
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name");
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user ID

            // Get users from the AD group "Scribe Admins"
            ViewData["Users"] = _adService.GetGroupMembersSelectList("Scribe Admins");


            return PartialView("_Create");
            //return View();
        }

        // POST: Maintenances/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Maintenance serviceHistory)
        {
            if (ModelState.IsValid)
            {
                //Updating the Condition of the device
                var sn = await _context.SerialNumbers.FindAsync(serviceHistory.SerialNumberId);
                sn.ConditionId = serviceHistory.ConditionId;
                _context.Update(sn);

                if (serviceHistory.SystemUserId == "")
                {
                    serviceHistory.SystemUserId = User.Identity.Name;
                }

                if(serviceHistory.SystemUserId == null)
                {
                    serviceHistory.SystemUserId = User.Identity.Name;
                }

                _context.Add(serviceHistory);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Service Log Created Successfully";
                return RedirectToAction(nameof(Index));
            }
            ViewData["SerialNumberId"] = new SelectList(_context.SerialNumbers, "Id", "Name", serviceHistory.SerialNumberId);
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name");
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user ID

            // Get users from the AD group "Scribe Admins"
            ViewData["Users"] = _adService.GetGroupMembersSelectList("Scribe Admins");

            return PartialView("_Create");
        }

        // GET: Maintenances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {


            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Maintenance", Url = Url.Action("Index", "Maintenances"), IsActive = false },
                new BreadcrumbItem { Title = "Edit", Url = Url.Action("Edit", "Maintenances"), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;
            if (id == null)
            {
                return NotFound();
            }

            var serviceHistory = await _context.Maintenances.FindAsync(id);
            if (serviceHistory == null)
            {
                return NotFound();
            }
            ViewData["SerialNumberId"] = new SelectList(_context.SerialNumbers, "Id", "Name", serviceHistory.SerialNumberId);
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name");
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user ID

            // Get users from the AD group "Scribe Admins"
            ViewData["Users"] = _adService.GetGroupMembersSelectList("Scribe Admins");


            return PartialView("_Edit", serviceHistory);
        }

        // POST: Maintenances/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Maintenance serviceHistory)
        {
            if (id != serviceHistory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (serviceHistory.SystemUserId == "")
                    {
                        serviceHistory.SystemUserId = User.Identity.Name;
                    }


                    if (serviceHistory.SystemUserId == null)
                    {
                        serviceHistory.SystemUserId = User.Identity.Name;
                    }

                    //Updating Device Condition
                    var sn = await _context.SerialNumbers.FindAsync(serviceHistory.SerialNumberId);
                    sn.ConditionId = serviceHistory.ConditionId;
                    _context.Update(sn);

                    _context.Update(serviceHistory);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Maintenance Log Updated Successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceHistoryExists(serviceHistory.Id))
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
            ViewData["SerialNumberId"] = new SelectList(_context.SerialNumbers, "Id", "Name", serviceHistory.SerialNumberId);
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name");
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user ID

            // Get users from the AD group "Scribe Admins"
            ViewData["Users"] = _adService.GetGroupMembersSelectList("Scribe Admins");


            return PartialView("_Edit", serviceHistory);
        }

        // GET: Maintenances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {


            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Maintenance", Url = Url.Action("Index", "Maintenances"), IsActive = false },
                new BreadcrumbItem { Title = "Delete", Url = Url.Action("Delete", "Maintenances"), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            if (id == null)
            {
                return NotFound();
            }

            var serviceHistory = await _context.Maintenances
                .Include(s => s.SerialNumber)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serviceHistory == null)
            {
                return NotFound();
            }

            return PartialView("_Delete", serviceHistory);
        }

        // POST: Maintenances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serviceHistory = await _context.Maintenances.FindAsync(id);
            if (serviceHistory != null)
            {
                _context.Maintenances.Remove(serviceHistory);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Maintenance Log Deleted Successfully";
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceHistoryExists(int id)
        {
            return _context.Maintenances.Any(e => e.Id == id);
        }

        //JavaScript Returns

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

        // New action to get condition by SerialNumberId
        public IActionResult GetConditionBySerialNumber(int serialNumberId)
        {
            var condition = _context.SerialNumbers
                .Where(sn => sn.Id == serialNumberId)
                .Select(sn => new
                {
                    Id = sn.ConditionId, // Assuming you have a ConditionId in your SerialNumber model
                    Name = sn.Condition.Name // Assuming you have navigation property for Condition
                })
                .FirstOrDefault();

            return Json(condition);
        }
    }
}
