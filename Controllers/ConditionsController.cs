using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scribe.Data;
using Scribe.Models;
using Scribe.Services;

namespace Scribe.Controllers
{
    public class ConditionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;

        public ConditionsController(ApplicationDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        // GET: Conditions
        public async Task<IActionResult> Index()
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Conditions", Url = Url.Action("Index", "Conditions"), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(await _context.Condition.ToListAsync());
        }

        // GET: Conditions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Conditions", Url = Url.Action("Index", "Conditions"), IsActive = false },
                new BreadcrumbItem { Title = "Details", Url = Url.Action("Details", "Conditions", new { id }), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            if (id == null)
            {
                TempData["Failure"] = "Condition ID is required.";
                return NotFound();
            }

            var condition = await _context.Condition
                .FirstOrDefaultAsync(m => m.Id == id);
            if (condition == null)
            {
                TempData["Failure"] = "Condition not found.";
                return NotFound();
            }

            return View(condition);
        }

        // GET: Conditions/Create
        public IActionResult Create()
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Conditions", Url = Url.Action("Index", "Conditions"), IsActive = false },
                new BreadcrumbItem { Title = "Create", Url = Url.Action("Create", "Conditions"), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("_Create");
        }

        // POST: Conditions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ColorCode")] Condition condition)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate Name (case-insensitive)
                if (_context.Condition.Any(c => c.Name.ToLower() == condition.Name.ToLower()))
                {
                    TempData["Failure"] = $"A condition with the name '{condition.Name}' already exists.";
                    return RedirectToAction(nameof(Index));
                }

                try
                {
                    _context.Add(condition);
                    await _context.SaveChangesAsync();

                    // Log entry
                    var log = new Log
                    {
                        Details = $"Condition {condition.Name} with ColorCode {condition.ColorCode} created.",
                        User = User.Identity.Name ?? "Anonymous",
                        Date = DateTime.Now
                    };

                    _context.Add(log);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Condition created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Failure"] = $"Failed to create condition. Error: {ex.Message}";
                }
            }
            else
            {
                TempData["Failure"] = "Failed to create condition. Please check the form for errors.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Conditions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Conditions", Url = Url.Action("Index", "Conditions"), IsActive = false },
                new BreadcrumbItem { Title = "Edit", Url = Url.Action("Edit", "Conditions", new { id }), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            if (id == null)
            {
                TempData["Failure"] = "Condition ID is required.";
                return NotFound();
            }

            var condition = await _context.Condition.FindAsync(id);
            if (condition == null)
            {
                TempData["Failure"] = "Condition not found.";
                return NotFound();
            }
            return PartialView("_Edit", condition);
        }

        // POST: Conditions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ColorCode")] Condition condition)
        {
            if (id != condition.Id)
            {
                TempData["Failure"] = "Condition ID mismatch.";
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Check for duplicate Name (case-insensitive) excluding the current condition
                if (_context.Condition.Any(c => c.Name.ToLower() == condition.Name.ToLower() && c.Id != id))
                {
                    TempData["Failure"] = $"A condition with the name '{condition.Name}' already exists.";
                    return PartialView("_Edit", condition);
                }

                try
                {
                    var originalCondition = await _context.Condition.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (originalCondition == null)
                    {
                        TempData["Failure"] = "Condition not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Update(condition);
                    await _context.SaveChangesAsync();

                    // Log entry
                    var log = new Log
                    {
                        Details = $"Condition updated from Name: {originalCondition.Name}, ColorCode: {originalCondition.ColorCode} to Name: {condition.Name}, ColorCode: {condition.ColorCode}",
                        User = User.Identity.Name ?? "Anonymous",
                        Date = DateTime.Now
                    };

                    _context.Add(log);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Condition updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConditionExists(condition.Id))
                    {
                        TempData["Failure"] = "Condition not found.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["Failure"] = "Failed to update condition due to a concurrency issue.";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["Failure"] = $"Failed to update condition. Error: {ex.Message}";
                }
            }
            else
            {
                TempData["Failure"] = "Failed to update condition. Please check the form for errors.";
            }

            return PartialView("_Edit", condition);
        }

        // GET: Conditions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Conditions", Url = Url.Action("Index", "Conditions"), IsActive = false },
                new BreadcrumbItem { Title = "Delete", Url = Url.Action("Delete", "Conditions", new { id }), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            if (id == null)
            {
                TempData["Failure"] = "Condition ID is required.";
                return NotFound();
            }

            var condition = await _context.Condition
                .FirstOrDefaultAsync(m => m.Id == id);
            if (condition == null)
            {
                TempData["Failure"] = "Condition not found.";
                return NotFound();
            }

            return PartialView("_Delete", condition);
        }

        // POST: Conditions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var condition = await _context.Condition.FindAsync(id);
            if (condition != null)
            {
                try
                {
                    _context.Condition.Remove(condition);
                    await _context.SaveChangesAsync();

                    // Log entry
                    var log = new Log
                    {
                        Details = $"Condition {condition.Name} with Color Code {condition.ColorCode} deleted.",
                        User = User.Identity.Name ?? "Anonymous",
                        Date = DateTime.Now
                    };

                    _context.Add(log);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Condition deleted successfully.";
                }
                catch (Exception ex)
                {
                    TempData["Failure"] = $"Failed to delete condition. Error: {ex.Message}";
                }
            }
            else
            {
                TempData["Failure"] = "Condition not found.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ConditionExists(int id)
        {
            return _context.Condition.Any(e => e.Id == id);
        }
    }
}
