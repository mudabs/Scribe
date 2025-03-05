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
    public class ConditionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;

        public ConditionsController(ApplicationDbContext context,ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        // GET: Conditions
        public async Task<IActionResult> Index()
        {
            return View(await _context.Condition.ToListAsync());
        }

        // GET: Conditions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var condition = await _context.Condition
                .FirstOrDefaultAsync(m => m.Id == id);
            if (condition == null)
            {
                return NotFound();
            }

            return View(condition);
        }

        // GET: Conditions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Conditions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ColorCode")] Condition condition)
        {
            if (ModelState.IsValid)
            {
                _context.Add(condition);
                await _context.SaveChangesAsync();

                // Create a log entry
                var log = new Log
                {
                    Details = $"Condition {condition.Name} with ColorCode {condition.ColorCode} created.",
                    User = User.Identity.Name ?? "Anonymous", // Assuming you have user authentication
                    Date = DateTime.Now
                };

                // Add the log entry to the database
                _context.Add(log);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(condition);
        }

        // GET: Conditions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var condition = await _context.Condition.FindAsync(id);
            if (condition == null)
            {
                return NotFound();
            }
            return View(condition);
        }

        // POST: Conditions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ColorCode")] Condition condition)
        {
            if (id != condition.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the original condition
                    var originalCondition = await _context.Condition.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (originalCondition == null)
                    {
                        return NotFound();
                    }

                    _context.Update(condition);
                    await _context.SaveChangesAsync();

                    // Create a log entry
                    var log = new Log
                    {
                        Details = $"Condition updated from Name: {originalCondition.Name}, ColorCode: {originalCondition.ColorCode} to Name: {condition.Name}, ColorCode: {condition.ColorCode}",
                        User = User.Identity.Name ?? "Anonymous", // Assuming you have user authentication
                        Date = DateTime.Now
                    };

                    // Add the log entry to the database
                    _context.Add(log);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConditionExists(condition.Id))
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
            return View(condition);
        }

        // GET: Conditions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var condition = await _context.Condition
                .FirstOrDefaultAsync(m => m.Id == id);
            if (condition == null)
            {
                return NotFound();
            }

            return View(condition);
        }

        // POST: Conditions/Delete/5
        [HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    var condition = await _context.Condition.FindAsync(id);
    if (condition != null)
    {
        _context.Condition.Remove(condition);
        await _context.SaveChangesAsync();

        // Create a log entry
        var log = new Log
        {
            Details = $"Condition {condition.Name} with ColorCode {condition.ColorCode} deleted.",
            User = User.Identity.Name ?? "Anonymous", // Assuming you have user authentication
            Date = DateTime.Now
        };

        // Add the log entry to the database
        _context.Add(log);
        await _context.SaveChangesAsync();
    }

    return RedirectToAction(nameof(Index));
}

        private bool ConditionExists(int id)
        {
            return _context.Condition.Any(e => e.Id == id);
        }
    }
}
