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
using Scribe.Services;

namespace Scribe.Controllers
{
    public class ServiceLogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;

        public ServiceLogsController(ApplicationDbContext context,ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        // GET: ServiceLogs
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ServiceLogs.Include(s => s.SerialNumber);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ServiceLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceLog = await _context.ServiceLogs
                .Include(s => s.SerialNumber)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serviceLog == null)
            {
                return NotFound();
            }

            return View(serviceLog);
        }

        // GET: ServiceLogs/Create
        public IActionResult Create()
        {
            ViewData["SerialNumberId"] = new SelectList(_context.SerialNumbers, "Id", "Name");
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            return View();
        }

        // POST: ServiceLogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SerialNumberId,Description,Date")] ServiceLog serviceLog)
        {
            // Check for existing Category with the same Name
            bool exists = await _context.ServiceLogs
                .AnyAsync(m => m.Description == serviceLog.Description && m.SerialNumberId == serviceLog.SerialNumberId && m.Date == serviceLog.Date);

            if (exists)
            {
                TempData["Failure"] = "The service Log currently exists.";
                return RedirectToAction(nameof(Create));
            }

            if (ModelState.IsValid)
            {
                _context.Add(serviceLog);
                TempData["Success"] = "Service Log Added Successfully!!!";
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["SerialNumberId"] = new SelectList(_context.SerialNumbers, "Id", "Name", serviceLog.SerialNumberId);
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            return View(serviceLog);
        }

        // GET: ServiceLogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceLog = await _context.ServiceLogs.FindAsync(id);
            if (serviceLog == null)
            {
                return NotFound();
            }
            ViewData["SerialNumberId"] = new SelectList(_context.SerialNumbers, "Id", "Name", serviceLog.SerialNumberId);
            return View(serviceLog);
        }

        // POST: ServiceLogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SerialNumberId,Description,Date")] ServiceLog serviceLog)
        {

            // Check for existing Category with the same Name
            bool exists = await _context.ServiceLogs
                .AnyAsync(m => m.Description == serviceLog.Description && m.SerialNumberId == serviceLog.SerialNumberId && m.Date == serviceLog.Date);

            if (exists)
            {
                TempData["Failure"] = "The service Log currently exists.";
                return RedirectToAction(nameof(Create));
            }

            if (id != serviceLog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(serviceLog);
                    TempData["Success"] = "Service Log Edited Successfully!!!";
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceLogExists(serviceLog.Id))
                    {
                        TempData["Failure"] = "Error!!!";
                        return RedirectToAction(nameof(Edit));
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SerialNumberId"] = new SelectList(_context.SerialNumbers, "Id", "Name", serviceLog.SerialNumberId);
            return View(serviceLog);
        }

        // GET: ServiceLogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceLog = await _context.ServiceLogs
                .Include(s => s.SerialNumber)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serviceLog == null)
            {
                return NotFound();
            }

            return View(serviceLog);
        }

        // POST: ServiceLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serviceLog = await _context.ServiceLogs.FindAsync(id);
            if (serviceLog != null)
            {
                _context.ServiceLogs.Remove(serviceLog);
            }

            TempData["Success"] = "Service Log Deleted Successfully!!!";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        [HttpGet("api/models")]
        public async Task<IActionResult> GetModels(int brandId)
        {
            var models = await _context.Models
                .Where(m => m.BrandId == brandId)
                .Select(m => new { m.Id, m.Name })
                .ToListAsync();

            return Ok(models);
        }

        [HttpGet("api/serialnumbers")]
        public async Task<IActionResult> GetSerialNumbers(int modelId)
        {
            var serialNumbers = await _context.SerialNumbers
                .Where(sn => sn.ModelId == modelId)
                .Select(sn => new { sn.Id, sn.Name })
                .ToListAsync();

            return Ok(serialNumbers);
        }
        private bool ServiceLogExists(int id)
        {
            return _context.ServiceLogs.Any(e => e.Id == id);
        }
    }
}
