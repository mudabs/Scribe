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
using Scribe.Data;

namespace Scribe.Controllers
{
    public class LocationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;

        public LocationsController(ApplicationDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        // GET: Locations
        public async Task<IActionResult> Index()
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Locations", Url = Url.Action("Index", "Locations"), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(await _context.Locations.ToListAsync());
        }

        // GET: Locations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Locations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (location == null)
            {
                return NotFound();
            }

            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Locations", Url = Url.Action("Index", "Locations"), IsActive = false },
                new BreadcrumbItem { Title = "Details", Url = Url.Action("Details", "Locations", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(location);
        }

        // GET: Locations/Create
        public IActionResult Create()
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Locations", Url = Url.Action("Index", "Locations"), IsActive = false },
                new BreadcrumbItem { Title = "Create", Url = Url.Action("Create", "Locations"), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("_Create");
        }

        // POST: Locations/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Location location)
        {
            if (ModelState.IsValid)
            {
                _context.Add(location);

                // Create a log entry using logging service
                var details = $"Location: {location.Name} created.";
                var myUser = User.Identity.Name; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action

                TempData["Success"] = "Location Created Successfully!!";
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return PartialView("_Create");
        }

        // GET: Locations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Locations.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Locations", Url = Url.Action("Index", "Locations"), IsActive = false },
                new BreadcrumbItem { Title = "Edit", Url = Url.Action("Edit", "Locations", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("_Edit", location);
        }

        // POST: Locations/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Location location)
        {
            if (id != location.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(location);

                    // Create a log entry using logging service
                    var details = $"Brand: {location.Name} updated.";
                    var myUser = User.Identity.Name; // Assuming you have user authentication
                    await _loggingService.LogActionAsync(details, myUser); // Log the action

                    TempData["Success"] = "Location Updated Successfully!!";
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocationExists(location.Id))
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
            return View(location);
        }

        // GET: Locations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _context.Locations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (location == null)
            {
                return NotFound();
            }

            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Locations", Url = Url.Action("Index", "Locations"), IsActive = false },
                new BreadcrumbItem { Title = "Delete", Url = Url.Action("Delete", "Locations", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("_Delete", location);
        }

        // POST: Locations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location != null)
            {
                // Create a log entry using logging service
                var details = $"Brand: {location.Name} deleted.";
                var myUser = User.Identity.Name; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action

                _context.Locations.Remove(location);

                TempData["Success"] = "Location Deleted Successfully!!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LocationExists(int id)
        {
            return _context.Locations.Any(e => e.Id == id);
        }
    }
}
