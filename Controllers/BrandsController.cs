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
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
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
            return View(await _context.Brands.ToListAsync());
        }

        // GET: Brands/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands
                .FirstOrDefaultAsync(m => m.Id == id);
            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }

        // GET: Brands/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Brands/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brand brand, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload
                if (imageFile != null && IsImageValid(imageFile))
                {
                    brand.ImageName = await SaveImageAsync(imageFile);
                }
                // Add the brand to the database
                _context.Add(brand);
                await _context.SaveChangesAsync();

                // Create a log entry using logging service
                var details = $"Brand: {brand.Name} created.";
                var myUser = User.Identity.Name; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(brand);
        }

        // GET: Brands/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands.FindAsync(id);
            if (brand == null)
            {
                return NotFound();
            }
            return View(brand);
        }

        // POST: Brands/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Brand brand)
        {
            if (id != brand.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the initial brand from the database
                    var originalBrand = await _context.Brands.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
                    var initialBrandName = originalBrand.Name;
                    if (originalBrand == null)
                    {
                        return NotFound();
                    }

                    var imageName = originalBrand.Name;


                    if (brand.ImageName != null)
                    {
                        string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/brands");
                        imageName = Guid.NewGuid().ToString() + "_" + brand.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadDir, imageName);
                        using (var fs = new FileStream(filePath, FileMode.Create))
                        {
                            await brand.ImageUpload.CopyToAsync(fs);
                        }
                        brand.ImageName = imageName;
                    }


                    brand.ImageName = originalBrand.ImageName; // Keep the old image name
                    // Update the brand
                    _context.Update(brand);
                    await _context.SaveChangesAsync();

                    // Create a log entry using logging service
                    var details = $"Brand: {initialBrandName} Updated to {brand.Name}.";
                    var myUser = User.Identity.Name; // Assuming you have user authentication
                    await _loggingService.LogActionAsync(details, myUser); // Log the action

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BrandExists(brand.Id))
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
            return View(brand);
        }

        // GET: Brands/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brand = await _context.Brands
                .FirstOrDefaultAsync(m => m.Id == id);
            if (brand == null)
            {
                return NotFound();
            }

            return View(brand);
        }

        // POST: Brands/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var brand = await _context.Brands.FindAsync(id);
            if (brand != null)
            {


                // Create a log entry using logging service
                var details = $"Brand: {brand.Name} Deleted.";
                var myUser = User.Identity.Name; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action

                DeleteImage(brand.ImageName); // Delete the image from the file system
                _context.Brands.Remove(brand);


                await _context.SaveChangesAsync();
            }

            await _context.SaveChangesAsync();
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
