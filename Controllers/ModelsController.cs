using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Scribe.Data;
using Scribe.Models;
using Scribe.Services;
using System.Security.Claims;

namespace Scribe.Controllers
{
    [Authorize(Policy = "GroupPolicy")]
    public class ModelsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILoggingService _loggingService;
        private readonly IActiveDirectoryService _adService;

        public ModelsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILoggingService loggingService, IActiveDirectoryService adService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _loggingService = loggingService;
            _adService = adService;
        }

        // GET: Models
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Models
                .Include(m => m.Brand)
                .Include(m => m.Category)
                .Include(m => m.SerialNumbers);


            ViewBag.BrandId = _context.Brands
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Name
                })
                .ToList();

            ViewBag.CategoryId = _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();

            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Models", Url = Url.Action("Index", "Models"), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Models/ModelByBrand/5 
        public async Task<IActionResult> ModelByBrand(int? id)
        {
            var brand = await _context.Brands.FindAsync(id);
            var applicationDbContext = _context.Models
                .Include(m => m.Brand)
                .Include(m => m.Category)
                .Where(m => m.BrandId == id);

            ViewBag.ThisBrandId = id;

            ViewBag.BrandId = _context.Brands
                 .Select(b => new SelectListItem
                 {
                     Value = b.Id.ToString(),
                     Text = b.Name
                 })
                 .ToList();

            ViewBag.CategoryId = _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();

            ViewBag.BrandName = brand.Name;

            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Models", Url = Url.Action("Index", "Models"), IsActive = false },
                new BreadcrumbItem { Title = brand.Name, Url = Url.Action("ModelByBrand", "Models", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Models/ModelByCategory/5 
        public async Task<IActionResult> ModelByCategory(int? id)
        {
            var category = await _context.Categories.FindAsync(id);
            var applicationDbContext = _context.Models
                .Include(m => m.Brand)
                .Include(m => m.Category)
                .Where(m => m.CategoryId == id);

            ViewBag.BrandId = _context.Brands
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Name
                })
                .ToList();

            ViewBag.CategoryId = _context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToList();

            ViewBag.CategoryName = category.Name;
            ViewBag.ThisCategoryId = id;

            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Models", Url = Url.Action("Index", "Models"), IsActive = false },
                new BreadcrumbItem { Title = category.Name, Url = Url.Action("ModelByCategory", "Models", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Models/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _context.Models
                .Include(m => m.Brand)
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (model == null)
            {
                return NotFound();
            }

            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Models", Url = Url.Action("Index", "Models"), IsActive = false },
                new BreadcrumbItem { Title = model.Name, Url = Url.Action("Details", "Models", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(model);
        }

        // GET: Models/Create
        public IActionResult Create()
        {
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");

            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Models", Url = Url.Action("Index", "Models"), IsActive = false },
                new BreadcrumbItem { Title = "Create", Url = Url.Action("Create", "Models"), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("_Create");
        }

        // POST: Models/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Model model)
        {
            bool exists = await _context.Models
                .AnyAsync(m => m.BrandId == model.BrandId && m.Name == model.Name && m.CategoryId == model.CategoryId);

            if (exists)
            {
                TempData["Failure"] = "A model with the same Brand, Name, and Category already exists.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                string imageName = "noimage.png";

                if (model.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/models");
                    imageName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);
                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageUpload.CopyToAsync(fs);
                    }
                    model.Image = imageName;
                }

                _context.Add(model);
                await _context.SaveChangesAsync();

                var warranty = new Warranty
                {
                    ModelId = model.Id,
                    PurchaseDate = DateTime.UtcNow,
                    WarrantyDurationYears = 0
                };
                _context.Warranties.Add(warranty);
                await _context.SaveChangesAsync();

                var details = $"Model {model.Name} created with Image {imageName}.";
                var myUser = User.Identity.Name ?? "Anonymous";
                await _loggingService.LogActionAsync(details, myUser);

                TempData["Success"] = "The Model has been created Successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", model.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        // GET: Models/CreateByBrand/5
        public IActionResult CreateByBrand(int id)
        {
            int brandId = id;
            var brand = _context.Brands.Find(brandId);

            if (brand == null)
            {
                return NotFound();
            }

            var model = new Model
            {
                BrandId = brandId
            };

            ViewData["BrandName"] = brand.Name;
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");

            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Models", Url = Url.Action("Index", "Models"), IsActive = false },
                new BreadcrumbItem { Title = brand.Name, Url = Url.Action("ModelByBrand", "Models", new { id }), IsActive = false },
                new BreadcrumbItem { Title = "Create", Url = Url.Action("CreateByBrand", "Models", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("_CreateByBrand", model);
        }

        // POST: Models/CreateByBrand/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateByBrand(Model model)
        {
            bool exists = await _context.Models
                .AnyAsync(m => m.BrandId == model.BrandId && m.Name == model.Name && m.CategoryId == model.CategoryId);

            if (exists)
            {
                TempData["Failure"] = "A model with the same Brand, Name, and Category already exists.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                string imageName = "noimage.png";

                if (model.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/models");
                    imageName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);
                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageUpload.CopyToAsync(fs);
                    }
                    model.Image = imageName;
                }

                _context.Add(model);
                await _context.SaveChangesAsync();

                var details = $"Model {model.Name} created with Image {imageName}.";
                var myUser = User.Identity.Name ?? "Anonymous";
                await _loggingService.LogActionAsync(details, myUser);

                TempData["Success"] = "The Model has been created Successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", model.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        // GET: Models/CreateByCategory/5
        public IActionResult CreateByCategory(int id)
        {
            int categoryId = id;
            var category = _context.Categories.Find(categoryId);

            if (category == null)
            {
                return NotFound();
            }

            var model = new Model
            {
                CategoryId = categoryId
            };

            ViewData["CategoryName"] = category.Name;
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");

            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Models", Url = Url.Action("Index", "Models"), IsActive = false },
                new BreadcrumbItem { Title = category.Name, Url = Url.Action("ModelByCategory", "Models", new { id }), IsActive = false },
                new BreadcrumbItem { Title = "Create", Url = Url.Action("CreateByCategory", "Models", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("_CreateByCategory", model);
        }

        // POST: Models/CreateByCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateByCategory(Model model)
        {
            bool exists = await _context.Models
               .AnyAsync(m => m.BrandId == model.BrandId && m.Name == model.Name && m.CategoryId == model.CategoryId);

            if (exists)
            {
                TempData["Failure"] = "A model with the same Brand, Name, and Category already exists.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                string imageName = "noimage.png";

                if (model.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/models");
                    imageName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadDir, imageName);
                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageUpload.CopyToAsync(fs);
                    }
                    model.Image = imageName;
                }

                _context.Add(model);
                await _context.SaveChangesAsync();

                var details = $"Model {model.Name} created with Image {imageName}.";
                var myUser = User.Identity.Name ?? "Anonymous";
                await _loggingService.LogActionAsync(details, myUser);

                TempData["Success"] = "The Model has been created Successfully!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", model.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            return View(model);
        }

        // GET: Models/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var originalModel = await _context.Models.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            if (id == null)
            {
                return NotFound();
            }

            var model = await _context.Models.FindAsync(id);
            if (model == null)
            {
                return NotFound();
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", model.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);

            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Models", Url = Url.Action("Index", "Models"), IsActive = false },
                new BreadcrumbItem { Title = model.Name, Url = Url.Action("Edit", "Models", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("_Edit", model);
        }

        // POST: Models/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Model model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalModel = await _context.Models.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
                    if (originalModel == null)
                    {
                        return NotFound();
                    }

                    string oldImage = originalModel.Image;
                    string imageName = oldImage;

                    if (model.ImageUpload != null)
                    {
                        string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/devices");
                        imageName = Guid.NewGuid().ToString() + "_" + model.ImageUpload.FileName;
                        string filePath = Path.Combine(uploadDir, imageName);

                        using (var fs = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImageUpload.CopyToAsync(fs);
                        }
                    }
                    model.Image = imageName;

                    _context.Update(model);
                    await _context.SaveChangesAsync();

                    var log = new Log
                    {
                        Details = $"Model updated from Name: {originalModel.Name}, Image: {oldImage} to Name: {model.Name}, Image: {imageName}.",
                        User = User.Identity.Name ?? "Anonymous",
                        Date = DateTime.Now
                    };

                    TempData["Success"] = "The Model has been edited Successfully!";
                    _context.Add(log);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModelExists(model.Id))
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
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", model.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);

            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Models", Url = Url.Action("Index", "Models"), IsActive = false },
                new BreadcrumbItem { Title = model.Name, Url = Url.Action("Edit", "Models", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(model);
        }

        // GET: Models/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var model = await _context.Models
                .Include(m => m.Brand)
                .Include(m => m.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (model == null)
            {
                return NotFound();
            }

            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Models", Url = Url.Action("Index", "Models"), IsActive = false },
                new BreadcrumbItem { Title = model.Name, Url = Url.Action("Delete", "Models", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("_Delete", model);
        }

        // POST: Models/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var model = await _context.Models.FindAsync(id);
            if (model != null)
            {
                int linkedSerialNumbersCount = await _context.SerialNumbers.CountAsync(sn => sn.ModelId == id);
                if (linkedSerialNumbersCount > 0)
                {
                    TempData["Failure"] = $"Cannot delete the model as there are {linkedSerialNumbersCount} linked Serial Numbers.";
                    return RedirectToAction("Index", "Models");
                }

                _context.Models.Remove(model);
                DeleteImage(model.Image);
                await _context.SaveChangesAsync();

                var details = $"Model {model.Name} with Image {model.Image} deleted.";
                var myUser = User.Identity.Name ?? "Anonymous";
                await _loggingService.LogActionAsync(details, myUser);

                TempData["Success"] = "The Model has been deleted Successfully!";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Models");
        }

        private bool ModelExists(int id)
        {
            return _context.Models.Any(e => e.Id == id);
        }

        // Action to render the view component
        public IActionResult AllocateSerialNumber(int id)
        {
            var count = _context.SerialNumbers.Count(sn => sn.ModelId == id);

            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name");
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Name");
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name");
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Name");
            ViewData["modelId"] = id;

            var currentUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["Users"] = _adService.GetGroupMembersSelectList("Scribe Admins");

            var breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                    new BreadcrumbItem { Title = "Models", Url = Url.Action("Index", "Models"), IsActive = false },
                    new BreadcrumbItem { Title = "Allocate Serial Number", Url = Url.Action("AllocateSerialNumber", "Models", new { id }), IsActive = true }
                };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return ViewComponent("SerialNumber", new { modelId = id, count = count });
        }

        // Action to handle form submission
        [HttpPost]
        public async Task<IActionResult> CreateSerialNumber(int modelId, string name)
        {
            bool exists = await _context.SerialNumbers
                .AnyAsync(sn => sn.ModelId == modelId && sn.Name == name);

            if (exists)
            {
                TempData["Failure"] = "Serial Number already exists for this model!";
                return RedirectToAction("AllocateSerialNumber", new { id = modelId });
            }

            var serialNumber = new SerialNumber
            {
                ModelId = modelId,
                Name = name,
                ADUsersId = 1,
                ConditionId = 1,
                DepartmentId = 1,
                LocationId = 1,
                Creation = DateTime.Now,
                Allocation = null,
                AllocatedBy = User.Identity.Name,
                DeallocatedBy = null
            };

            if (ModelState.IsValid)
            {
                _context.Add(serialNumber);
                await _context.SaveChangesAsync();
                TempData["Success"] = "New Serial Number added successfully!!!";

                var model = await _context.Models.FindAsync(modelId);
                var brand = await _context.Brands.FindAsync(model.BrandId);
                var category = await _context.Categories.FindAsync(model.CategoryId);

                var details = $"Serial Number '{serialNumber.Name}' for '{brand.Name}' '{model.Name}' '{category.Name}' added.";
                var myUser = User.Identity.Name ?? "Anonymous";
                await _loggingService.LogActionAsync(details, myUser);
            }
            else
            {
                TempData["Failure"] = "Failed to add Serial Number!!!";
            }

            return RedirectToAction("AllocateSerialNumber", new { id = modelId });
        }

        // POST: Models/EditSerialNumber
        [HttpPost]
        public IActionResult EditSerialNumber(SerialNumber model, string name)
        {
            int? modelId = 0;

            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name");
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Name");
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name");
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Name");

            var serialNumber = _context.SerialNumbers
                .Include(sn => sn.Model)
                .Include(sn => sn.Condition)
                .Include(sn => sn.Department)
                .Include(sn => sn.Location)
                .Include(sn => sn.ADUsers)
                .FirstOrDefault(sn => sn.Id == model.Id);
            if (serialNumber == null)
            {
                return NotFound();
            }
            modelId = serialNumber.ModelId;

            // Update properties
            serialNumber.Name = model.Name;
            serialNumber.ConditionId = model.ConditionId;
            serialNumber.DepartmentId = model.DepartmentId;
            serialNumber.LocationId = model.LocationId;
            serialNumber.Creation = model.Creation;
            serialNumber.Allocation = model.Allocation;
            serialNumber.ADUsersId = model.ADUsersId;

            _context.Update(serialNumber);
            _context.SaveChanges();

            TempData["Success"] = "Serial Number Edited successfully!!!";

            var details = $"Serial Number '{serialNumber.Name}' for '{serialNumber.Model.Brand.Name}' '{serialNumber.Model.Name}' '{serialNumber.Model.Category.Name}' properties updated.";
            var myUser = User.Identity.Name ?? "Anonymous";
            _loggingService.LogActionAsync(details, myUser);

            var breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                    new BreadcrumbItem { Title = "Models", Url = Url.Action("Index", "Models"), IsActive = false },
                    new BreadcrumbItem { Title = "Edit Serial Number", Url = Url.Action("EditSerialNumber", "Models", new { id = model.Id }), IsActive = true }
                };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return RedirectToAction("AllocateSerialNumber", new { id = modelId });
        }

        // POST: Models/DeleteSerialNumber
        [HttpPost]
        public async Task<IActionResult> DeleteSerialNumber(int id, int modelId)
        {
            if (ModelState.IsValid)
            {
                var serialNumber = await _context.SerialNumbers.FindAsync(id);
                if (serialNumber == null)
                {
                    TempData["Failure"] = "Failed to Delete Device";
                    return RedirectToAction("AllocateSerialNumber", new { id = modelId });
                }

                var model = await _context.Models.FindAsync(serialNumber.ModelId);
                var brand = await _context.Brands.FindAsync(model.BrandId);
                var category = await _context.Categories.FindAsync(model.CategoryId);

                var details = $"Serial Number '{serialNumber.Name}' for '{brand.Name}' '{model.Name}' '{category.Name}' Deleted.";
                var myUser = User.Identity.Name ?? "Anonymous";
                await _loggingService.LogActionAsync(details, myUser);

                _context.SerialNumbers.Remove(serialNumber);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Serial Number Deleted successfully!!!";
            }
            else
            {
                TempData["Failure"] = "Failed to delete Serial Number!!!";
            }

            var breadcrumbs = new List<BreadcrumbItem>
                {
                    new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                    new BreadcrumbItem { Title = "Models", Url = Url.Action("Index", "Models"), IsActive = false },
                    new BreadcrumbItem { Title = "Delete Serial Number", Url = Url.Action("DeleteSerialNumber", "Models", new { id }), IsActive = true }
                };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return RedirectToAction("AllocateSerialNumber", new { id = modelId });
        }

        private void DeleteImage(string imageName)
        {
            if (!string.IsNullOrEmpty(imageName))
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "media/models", imageName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
        }

        //Warranty Information

        public async Task<IActionResult> WarrantyDetails(int modelId)
        {
            var warranty = await _context.Warranties
                .Include(w => w.Model)
                .FirstOrDefaultAsync(w => w.ModelId == modelId);

            if (warranty == null)
            {
                warranty = new Warranty
                {
                    ModelId = modelId,
                    PurchaseDate = DateTime.UtcNow,
                    WarrantyDurationYears = 1
                };
                _context.Warranties.Add(warranty);
                await _context.SaveChangesAsync();
            }

            return PartialView("_WarrantyDetailsPartial", warranty);
        }

        public async Task<IActionResult> EditWarranty(int modelId)
        {
            var warranty = await _context.Warranties
                .Include(w => w.Model)
                .FirstOrDefaultAsync(w => w.ModelId == modelId);

            if (warranty == null) return NotFound();

            return PartialView("_WarrantyEditPartial", warranty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditWarranty(Warranty warranty)
        {
            if (ModelState.IsValid)
            {
                _context.Warranties.Update(warranty);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return PartialView("_WarrantyEditPartial", warranty);
        }

    }
}

