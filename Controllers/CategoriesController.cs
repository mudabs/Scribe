using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Scribe.Infrastructure;
using Scribe.Models;
using System.Collections;
using Microsoft.AspNetCore.Hosting;
using Scribe.Services;
using System.Drawing.Drawing2D;
using Scribe.Data;


namespace Inventory.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILoggingService _loggingService;

        public CategoriesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILoggingService loggingService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _loggingService = loggingService;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return PartialView("_Create");
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                // Check for existing Category with the same Name
                bool exists = await _context.Categories
                    .AnyAsync(m => m.Name == category.Name);

                if (exists)
                {
                    TempData["Failure"] = "A category with the same Name already exists.";
                    return RedirectToAction(nameof(Create));
                }

                string iconName = "noicon.svg";

                if (category.IconUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/icons");
                    iconName = Guid.NewGuid().ToString() + "_" + category.IconUpload.FileName;
                    string filePath = Path.Combine(uploadDir, iconName);
                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await category.IconUpload.CopyToAsync(fs);
                    }
                }

                category.Icon = iconName;
                TempData["Success"] = "Category Created Successfully!!";
                _context.Add(category);
                await _context.SaveChangesAsync();


                // Create a log entry using logging service
                var details = $"Category: {category.Name} Created.";
                var myUser = User.Identity.Name; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action


                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return PartialView("_Edit",category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Check for existing Category with the same Name
                bool exists = await _context.Categories
                    .AnyAsync(m => m.Name == category.Name);

                if (exists)
                {
                    TempData["Failure"] = "A category with the same Name already exists.";
                    return RedirectToAction(nameof(Create));
                }

                try
                {
                    // Retrieve the original category
                    var originalCategory = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (originalCategory == null)
                    {
                        return NotFound();
                    }

                    // Check for existing Category with the same Name, but exclude the current category
                    bool exists2 = await _context.Categories
                        .AnyAsync(m => m.Name.Equals(category.Name, StringComparison.OrdinalIgnoreCase) && m.Id != id);

                    if (exists2)
                    {
                        TempData["Failure"] = "A category with the same Name already exists.";
                        return RedirectToAction(nameof(Edit));
                    }

                    string iconName = originalCategory.Icon; // Preserve current icon if no new upload
                    if (category.IconUpload != null)
                    {
                        string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/icons");
                        iconName = Guid.NewGuid().ToString() + "_" + category.IconUpload.FileName;
                        string filePath = Path.Combine(uploadDir, iconName);
                        using (var fs = new FileStream(filePath, FileMode.Create))
                        {
                            await category.IconUpload.CopyToAsync(fs);
                        }
                    }

                    category.Icon = iconName;
                    _context.Update(category);
                    TempData["Success"] = "Category Updated Successfully!";
                    await _context.SaveChangesAsync();

                    // Create a log entry using logging service
                    var details = $"Category: {category.Name} Updated.";
                    var myUser = User.Identity.Name; // Assuming you have user authentication
                    await _loggingService.LogActionAsync(details, myUser); // Log the action

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            return PartialView("_Edit",category);
        }


        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return PartialView("_Delete",category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories' is null.");
            }

            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {

                // Create a log entry using logging service
                var details = $"Category: {category.Name} Deleted.";
                var myUser = User.Identity.Name; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action

                DeleteImage(category.Icon);
                _context.Categories.Remove(category);
                TempData["Success"] = "Category Deleted Successfully!!";
                await _context.SaveChangesAsync();

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        public IActionResult Filtering()
        {
            List<SelectListItem> icons = new()
            {
                new SelectListItem { Value = "<i class=\'bi bi-laptop'></i>", Text = "<i class='bi bi-laptop'>laptop</i>" },
                new SelectListItem { Value = "<i class='bi bi-pc'></i>", Text = "<i class='bi bi-pc'>pc</i>" },

            };

            ViewBag.Icons = icons;
            return View();
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }

        private void DeleteImage(string imageName)
        {
            if (!string.IsNullOrEmpty(imageName))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "media/icons", imageName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }
    }
}
