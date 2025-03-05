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
    public class AllocationHistoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;


        public AllocationHistoriesController(ApplicationDbContext context,ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        // GET: AllocationHistories
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.AllocationHistory.Include(a => a.ADUsers).Include(a => a.SerialNumber);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: AllocationHistories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var allocationHistory = await _context.AllocationHistory
                .Include(a => a.ADUsers)
                .Include(a => a.SerialNumber)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (allocationHistory == null)
            {
                return NotFound();
            }

            return View(allocationHistory);
        }

        // GET: AllocationHistories/Create
        public IActionResult Create()
        {
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Id");
            ViewData["SerialNumberId"] = new SelectList(_context.SerialNumbers, "Id", "Name");
            return View();
        }

        // POST: AllocationHistories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,SerialNumberId,ADUsersId,AllocationDate,DeallocationDate")] AllocationHistory allocationHistory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(allocationHistory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Id", allocationHistory.ADUsersId);
            ViewData["SerialNumberId"] = new SelectList(_context.SerialNumbers, "Id", "Name", allocationHistory.SerialNumberId);
            return View(allocationHistory);
        }

        // GET: AllocationHistories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var allocationHistory = await _context.AllocationHistory.FindAsync(id);
            if (allocationHistory == null)
            {
                return NotFound();
            }
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Id", allocationHistory.ADUsersId);
            ViewData["SerialNumberId"] = new SelectList(_context.SerialNumbers, "Id", "Name", allocationHistory.SerialNumberId);
            return View(allocationHistory);
        }

        // POST: AllocationHistories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SerialNumberId,ADUsersId,AllocationDate,DeallocationDate")] AllocationHistory allocationHistory)
        {
            if (id != allocationHistory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(allocationHistory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AllocationHistoryExists(allocationHistory.Id))
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
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Id", allocationHistory.ADUsersId);
            ViewData["SerialNumberId"] = new SelectList(_context.SerialNumbers, "Id", "Name", allocationHistory.SerialNumberId);
            return View(allocationHistory);
        }

        // GET: AllocationHistories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var allocationHistory = await _context.AllocationHistory
                .Include(a => a.ADUsers)
                .Include(a => a.SerialNumber)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (allocationHistory == null)
            {
                return NotFound();
            }

            return View(allocationHistory);
        }

        // POST: AllocationHistories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var allocationHistory = await _context.AllocationHistory.FindAsync(id);
            if (allocationHistory != null)
            {
                _context.AllocationHistory.Remove(allocationHistory);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AllocationHistoryExists(int id)
        {
            return _context.AllocationHistory.Any(e => e.Id == id);
        }
    }
}
