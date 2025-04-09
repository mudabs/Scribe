using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ExcelDataReader.Log;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Scribe.Data;
using Scribe.Infrastructure;
using Scribe.Models;
using Scribe.Services;
using Log = Scribe.Models.Log;

namespace Scribe.Controllers
{
    public class LogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;

        public LogsController(ApplicationDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        // GET: Logs
        public async Task<IActionResult> Index()
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Logs", Url = Url.Action("Index", "Logs"), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            var logs = await _context.Log
                .OrderByDescending(log => log.Date) // Replace 'Date' with your actual date property name
                .ToListAsync();
            return View(logs);
        }

        // GET: Logs/MyActivity
        public async Task<IActionResult> MyActivity()
        {
            var userId = User.Identity.Name; // Get the currently logged-in user's ID
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Logs", Url = Url.Action("Index", "Logs"), IsActive = false },
                new BreadcrumbItem { Title = "My Activity", Url = Url.Action("MyActivity", "Logs"), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            var logs = await _context.Log
                .Where(log => log.User == userId) // Filter logs by the user's ID
                .OrderByDescending(log => log.Date) // Replace 'Date' with your actual date property name
                .ToListAsync();
            return View(logs);
        }

        // GET: Logs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var log = await _context.Log
                .FirstOrDefaultAsync(m => m.Id == id);
            if (log == null)
            {
                return NotFound();
            }

            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Logs", Url = Url.Action("Index", "Logs"), IsActive = false },
                new BreadcrumbItem { Title = "Details", Url = Url.Action("Details", "Logs", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(log);
        }

        // GET: Logs/Create
        public IActionResult Create()
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Logs", Url = Url.Action("Index", "Logs"), IsActive = false },
                new BreadcrumbItem { Title = "Create", Url = Url.Action("Create", "Logs"), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("_Create");
        }

        // POST: Logs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Log log)
        {
            if (ModelState.IsValid)
            {
                _context.Add(log);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(log);
        }

        // GET: Logs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var log = await _context.Log.FindAsync(id);
            if (log == null)
            {
                return NotFound();
            }

            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Logs", Url = Url.Action("Index", "Logs"), IsActive = false },
                new BreadcrumbItem { Title = "Edit", Url = Url.Action("Edit", "Logs", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("_Edit", log);
            
        }

            // POST: Logs/Edit/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, [Bind("Id,Details,User,Date")] Log log)
            {
                if (id != log.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(log);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!LogExists(log.Id))
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
                return View(log);
            }

        // GET: Logs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var log = await _context.Log
                .FirstOrDefaultAsync(m => m.Id == id);
            if (log == null)
            {
                return NotFound();
            }

            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Logs", Url = Url.Action("Index", "Logs"), IsActive = false },
                new BreadcrumbItem { Title = "Delete", Url = Url.Action("Delete", "Logs", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("Delete", log);
        }

        // POST: Logs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var log = await _context.Log.FindAsync(id);
            if (log != null)
            {
                _context.Log.Remove(log);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LogExists(int id)
        {
            return _context.Log.Any(e => e.Id == id);
        }
    }
}
