using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Scribe.Data;
using Scribe.Infrastructure;
using Scribe.Models;


namespace Scribe.Controllers
{
    public class MaintenancesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MaintenancesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Maintenances
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Maintenances.Include(m => m.SerialNumber).Include(m => m.SerialNumber.Model).Include(m => m.SerialNumber.Model.Brand).Include(m => m.SerialNumber.Model.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Maintenances/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenance = await _context.Maintenances 
                .Include(m => m.SerialNumber)
                .Include(m => m.SerialNumber.Model)
                .Include(m => m.SerialNumber.Model.Brand)
                .Include(m => m.SerialNumber.Model.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (maintenance == null)
            {
                return NotFound();
            }

            return View(maintenance);
        }

        // GET: Maintenances/Create
        public IActionResult Create()
        {
            ViewData["SerialId"] = new SelectList(_context.SerialNumbers, "Id", "Name");
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name");
            return View();
        }

        // POST: Maintenances/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MaintenanceDate,NextMaintenance,Details,SerialId")] Maintenance maintenance)
        {
            if (ModelState.IsValid)
            {
                _context.Add(maintenance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SerialId"] = new SelectList(_context.SerialNumbers, "Id", "Name", maintenance.SerialId);
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name", maintenance.ConditionId);
            return View(maintenance);
        }

        // GET: Maintenances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenance = await _context.Maintenances.FindAsync(id);
            if (maintenance == null)
            {
                return NotFound();
            }
            ViewData["SerialId"] = new SelectList(_context.SerialNumbers, "Id", "Name", maintenance.SerialId);
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name");
            return View(maintenance);
        }

        // POST: Maintenances/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MaintenanceDate,NextMaintenance,Details,SerialId")] Maintenance maintenance)
        {
            if (id != maintenance.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(maintenance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaintenanceExists(maintenance.Id))
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
            ViewData["SerialId"] = new SelectList(_context.SerialNumbers, "Id", "Name", maintenance.SerialId);
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name", maintenance.ConditionId);
            return View(maintenance);
        }

        // GET: Maintenances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var maintenance = await _context.Maintenances
                .Include(m => m.SerialNumber)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (maintenance == null)
            {
                return NotFound();
            }

            return View(maintenance);
        }

        // POST: Maintenances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var maintenance = await _context.Maintenances.FindAsync(id);
            if (maintenance != null)
            {
                _context.Maintenances.Remove(maintenance);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MaintenanceExists(int id)
        {
            return _context.Maintenances.Any(e => e.Id == id);
        }

        //Java Script Returns

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
