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
    public class ServiceHistoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;
        private readonly UserManager<SystemUser> _userManager;
        private readonly IActiveDirectoryService _adService;

        public ServiceHistoriesController(ApplicationDbContext context,ILoggingService loggingService, UserManager<SystemUser> userManager, IActiveDirectoryService adService)
        {
            _context = context;
            _loggingService = loggingService;
            _userManager = userManager;
            _adService = adService;
        }

        // GET: ServiceHistories
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ServiceHistory.Include(s => s.SerialNumber).Include(s => s.Condition);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ServiceHistories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceHistory = await _context.ServiceHistory
                .Include(s => s.SerialNumber)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serviceHistory == null)
            {
                return NotFound();
            }

            return View(serviceHistory);
        }

        // GET: ServiceHistories/Create
        public IActionResult Create()
        {
            ViewData["SerialNumberId"] = new SelectList(_context.SerialNumbers, "Id", "Name");
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name");
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user ID

            // Get users from the AD group "ZIM-WEB-IT"
            ViewData["Users"] = _adService.GetGroupMembersSelectList("zim-web-it");


            return View();
        }

        // POST: ServiceHistories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceHistory serviceHistory)
        {
            if (ModelState.IsValid)
            {
                //Updating the Condition of the device
                var sn = await _context.SerialNumbers.FindAsync(serviceHistory.SerialNumberId);
                sn.ConditionId = serviceHistory.ConditionId;
                _context.Update(sn);

                if (serviceHistory.SystemUserId == "") {
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

            // Get users from the AD group "ZIM-WEB-IT"
            ViewData["Users"] = _adService.GetGroupMembersSelectList("zim-web-it");

            return View(serviceHistory);
        }

        // GET: ServiceHistories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceHistory = await _context.ServiceHistory.FindAsync(id);
            if (serviceHistory == null)
            {
                return NotFound();
            }
            ViewData["SerialNumberId"] = new SelectList(_context.SerialNumbers, "Id", "Name", serviceHistory.SerialNumberId);
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name");
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user ID

            // Get users from the AD group "ZIM-WEB-IT"
            ViewData["Users"] = _adService.GetGroupMembersSelectList("zim-web-it");


            return View(serviceHistory);
        }

        // POST: ServiceHistories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceHistory serviceHistory)
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
                    //Updating Device Condition
                    var sn = await _context.SerialNumbers.FindAsync(serviceHistory.SerialNumberId);
                    sn.ConditionId = serviceHistory.ConditionId;
                    _context.Update(sn);

                    _context.Update(serviceHistory);
                    await _context.SaveChangesAsync();
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

            // Get users from the AD group "ZIM-WEB-IT"
            ViewData["Users"] = _adService.GetGroupMembersSelectList("zim-web-it");


            return View(serviceHistory);
        }

        // GET: ServiceHistories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceHistory = await _context.ServiceHistory
                .Include(s => s.SerialNumber)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serviceHistory == null)
            {
                return NotFound();
            }

            return View(serviceHistory);
        }

        // POST: ServiceHistories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serviceHistory = await _context.ServiceHistory.FindAsync(id);
            if (serviceHistory != null)
            {
                _context.ServiceHistory.Remove(serviceHistory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceHistoryExists(int id)
        {
            return _context.ServiceHistory.Any(e => e.Id == id);
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
