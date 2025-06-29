﻿using System;
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

namespace Scribe.Controllers
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
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Categories", Url = Url.Action("Index", "Categories"), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(await _context.Categories.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Categories", Url = Url.Action("Index", "Categories"), IsActive = false },
                new BreadcrumbItem { Title = "Details", Url = Url.Action("Details", "Categories", new { id }), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Categories", Url = Url.Action("Index", "Categories"), IsActive = false },
                new BreadcrumbItem { Title = "Create", Url = Url.Action("Create", "Categories"), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("_Create");
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Categories", Url = Url.Action("Index", "Categories"), IsActive = false },
                new BreadcrumbItem { Title = "Create", Url = Url.Action("Create", "Categories"), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            if (ModelState.IsValid)
            {
                // Check for existing Category with the same Name
                bool exists = await _context.Categories.AnyAsync(m => m.Name == category.Name);

                if (exists)
                {
                    TempData["Failure"] = "A category with the same Name already exists.";
                    return RedirectToAction(nameof(Create));
                }

                string iconName = "noicon.svg";

                if (category.IconUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/icons/categories");
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

                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Categories", Url = Url.Action("Index", "Categories"), IsActive = false },
                new BreadcrumbItem { Title = "Edit", Url = Url.Action("Edit", "Categories", new { id }), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return PartialView("_Edit", category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Categories", Url = Url.Action("Index", "Categories"), IsActive = false },
                new BreadcrumbItem { Title = "Edit", Url = Url.Action("Edit", "Categories", new { id }), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the original category
                    var originalCategory = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (originalCategory == null)
                    {
                        return NotFound();
                    }

                    // Check for existing Category with the same Name
                    //bool exists2 = await _context.Categories.AnyAsync(m => m.Name == category.Name);

                    //if (exists2)
                    //{
                    //    TempData["Failure"] = "A category with the same Name already exists.";
                    //    return RedirectToAction(nameof(Index));
                    //}

                    string iconName = originalCategory.Icon; // Preserve current icon if no new upload
                    if (category.IconUpload != null)
                    {
                        string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/icons/categories");
                        iconName = Guid.NewGuid().ToString() + "_" + category.IconUpload.FileName;
                        string filePath = Path.Combine(uploadDir, iconName);
                        using (var fs = new FileStream(filePath, FileMode.Create))
                        {
                            await category.IconUpload.CopyToAsync(fs);
                        }
                        DeleteImage(originalCategory.Icon); // Delete the old image
                    }

                    category.Icon = iconName;
                    _context.Update(category);
                    TempData["Success"] = "Category Updated Successfully!";
                    await _context.SaveChangesAsync();

                    // Create a log entry using logging service
                    var details = $"Category: {category.Name} Updated.";
                    var myUser = User.Identity.Name; // Assuming you have user authentication
                    await _loggingService.LogActionAsync(details, myUser); // Log the action

                    return RedirectToAction(nameof(Index));
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
            }
            return PartialView("_Edit", category);
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }

        private void DeleteImage(string imageName)
        {
            if (!string.IsNullOrEmpty(imageName))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "media/icons/categories", imageName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Categories", Url = Url.Action("Index", "Categories"), IsActive = false },
                new BreadcrumbItem { Title = "Delete", Url = Url.Action("Delete", "Categories", new { id }), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            if (id == null)
            {
                TempData["Failure"] = "Category ID is required.";
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                TempData["Failure"] = "Categories not found.";
                return NotFound();
            }

            return PartialView("_Delete", category);
        }


        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index","Home"), IsActive = false },
                new BreadcrumbItem { Title = "Delete", Url = Url.Action("Delete", "Categories", new { id }), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

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
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Filtering()
        {
            List<SelectListItem> icons = new()
            {
                new SelectListItem { Value = "<i class='bi bi-laptop'></i>", Text = "<i class='bi bi-laptop'>laptop</i>" },
                new SelectListItem { Value = "<i class='bi bi-pc'></i>", Text = "<i class='bi bi-pc'>pc</i>" },
            };

            ViewBag.Icons = icons;
            return View();
        }
    }
}