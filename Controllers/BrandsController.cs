using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Scribe.Infrastructure;
using Scribe.Models;
using Microsoft.AspNetCore.Hosting;
using Scribe.Services;
using Scribe.Data;

namespace Scribe.Controllers
{
    public class BrandsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILoggingService _loggingService;

        public BrandsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILoggingService loggingService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _loggingService = loggingService;
        }

        // GET: Brands
        public async Task<IActionResult> Index()
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Brands", Url = Url.Action("Index", "Brands"), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(await _context.Brands.ToListAsync());
        }

        // GET: Brands/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Brands", Url = Url.Action("Index", "Brands"), IsActive = false },
                new BreadcrumbItem { Title = "Details", Url = Url.Action("Details", "Brands", new { id }), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands.FirstOrDefaultAsync(m => m.Id == id);
            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }

        // GET: Brands/Create
        public IActionResult Create()
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Brands", Url = Url.Action("Index", "Brands"), IsActive = false },
                new BreadcrumbItem { Title = "Create", Url = Url.Action("Create", "Brands"), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("_Create");
        }

        // POST: Brands/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand brand, IFormFile imageFile)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Brands", Url = Url.Action("Index", "Brands"), IsActive = false },
                new BreadcrumbItem { Title = "Create", Url = Url.Action("Create", "Brands"), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            if (ModelState.IsValid)
            {
                // Check if a brand with the same name already exists (case insensitive)
                if (_context.Brands.Any(b => b.Name.ToLower() == brand.Name.ToLower()))
                {
                    TempData["Failure"] = $"A brand with the name '{brand.Name}' already exists. Please choose a different name.";
                    return RedirectToAction(nameof(Index));
                }

                try
                {
                    // Handle image upload
                    if (imageFile != null && IsImageValid(imageFile))
                    {
                        brand.ImageName = await SaveImageAsync(imageFile);
                    }

                    // Add the brand to the database
                    _context.Add(brand);
                    await _context.SaveChangesAsync();

                    // Log the creation
                    var details = $"Brand: {brand.Name} created.";
                    var myUser = User.Identity.Name; // Assuming you have user authentication
                    await _loggingService.LogActionAsync(details, myUser);

                    TempData["Success"] = "Brand created successfully."; // Success message
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Failure"] = $"Failed to create brand. Error: {ex.Message}"; // Failure message
                }
            }
            else
            {
                TempData["Failure"] = "Failed to create brand. Please check the form for errors."; // Model state error
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Brands/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Brands", Url = Url.Action("Index", "Brands"), IsActive = false },
                new BreadcrumbItem { Title = "Edit", Url = Url.Action("Edit", "Brands", new { id }), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return PartialView("_Edit", brand);
        }

        // POST: Brands/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Brand brand)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Brands", Url = Url.Action("Index", "Brands"), IsActive = false },
                new BreadcrumbItem { Title = "Edit", Url = Url.Action("Edit", "Brands", new { id }), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            if (id != brand.Id)
            {
                TempData["Failure"] = "Invalid brand ID.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                // Check if a brand with the same name already exists (case insensitive)
                if (_context.Brands.Any(b => b.Name.ToLower() == brand.Name.ToLower()))
                {
                    TempData["Failure"] = $"A brand with the name '{brand.Name}' already exists. Please choose a different name.";
                    return RedirectToAction(nameof(Index));
                }

                try
                {
                    // Retrieve the initial brand from the database
                    var originalBrand = await _context.Brands.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
                    if (originalBrand == null)
                    {
                        TempData["Failure"] = "Brand not found.";
                        return RedirectToAction(nameof(Index));
                    }

                    var initialBrandName = originalBrand.Name;

                    // Update image if provided
                    if (brand.ImageName != null)
                    {
                        string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/brands");
                        var imageName = Guid.NewGuid().ToString() + "_" + brand.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadDir, imageName);
                        using (var fs = new FileStream(filePath, FileMode.Create))
                        {
                            await brand.ImageUpload.CopyToAsync(fs);
                        }
                        brand.ImageName = imageName;
                    }
                    else
                    {
                        brand.ImageName = originalBrand.ImageName; // Retain old image if none is provided
                    }

                    // Update the brand
                    _context.Update(brand);
                    await _context.SaveChangesAsync();

                    // Log the update
                    var details = $"Brand: {initialBrandName} updated to {brand.Name}.";
                    var myUser = User.Identity.Name; // Assuming you have user authentication
                    await _loggingService.LogActionAsync(details, myUser);

                    TempData["Success"] = "Brand updated successfully."; // Success message
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BrandExists(brand.Id))
                    {
                        TempData["Failure"] = "Brand not found.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["Failure"] = "Failed to update brand due to concurrency issue."; // Concurrency error
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    TempData["Failure"] = $"Failed to update brand. Error: {ex.Message}"; // Failure message
                }
            }
            else
            {
                TempData["Failure"] = "Failed to update brand. Please check the form for errors."; // Model state error
            }

            return PartialView("_Edit", brand);
        }

        // GET: Brands/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Brands", Url = Url.Action("Index", "Brands"), IsActive = false},
                new BreadcrumbItem { Title = "Delete", Url = Url.Action("Delete", "Brands", new { id }), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands.FirstOrDefaultAsync(m => m.Id == id);
            if (brand == null)
            {
                return NotFound();
            }

            return PartialView("_Delete", brand);
        }

        // POST: Brands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Brands", Url = Url.Action("Index", "Brands"), IsActive = false },
                new BreadcrumbItem { Title = "Delete", Url = Url.Action("Delete", "Brands", new { id }), IsActive = true }
            };

            ViewData["Breadcrumbs"] = breadcrumbs;

            var brand = await _context.Brands.FindAsync(id);
            if (brand != null)
            {
                try
                {
                    // Log the deletion
                    var details = $"Brand: {brand.Name} deleted.";
                    var myUser = User.Identity.Name; // Assuming you have user authentication
                    await _loggingService.LogActionAsync(details, myUser);

                    DeleteImage(brand.ImageName); // Delete the image from the file system
                    _context.Brands.Remove(brand);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Brand deleted successfully."; // Success message
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Failure"] = $"Failed to delete brand. Error: {ex.Message}"; // Failure message
                }
            }
            else
            {
                TempData["Failure"] = "Brand not found.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BrandExists(int id)
        {
            return _context.Brands.Any(e => e.Id == id);
        }

        // Helper methods
        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "media/brands");
            if (!Directory.Exists(uploads))
            {
                Directory.CreateDirectory(uploads);
            }

            var fileName = Path.GetFileName(imageFile.FileName);
            var filePath = Path.Combine(uploads, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return fileName;
        }

        private void DeleteImage(string imageName)
        {
            if (!string.IsNullOrEmpty(imageName))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "media/brands", imageName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

        private bool IsImageValid(IFormFile file)
        {
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".svg", ".JPG", ".JPEG", ".PNG", ".SVG" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return validExtensions.Contains(extension);
        }
    }
}
