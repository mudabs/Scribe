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

        public ModelsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, ILoggingService loggingService,IActiveDirectoryService adService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _loggingService = loggingService;
            _adService = adService;
        }

        // GET: Models
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Models.Include(m => m.Brand).Include(m => m.Category);

            // Populate ViewBag.Brands
            ViewBag.BrandId = _context.Brands // Assuming you have a DbContext named _context
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(), // Assuming Id is the primary key of your Brand model
                    Text = b.Name
                })
                .ToList();

            // Populate ViewBag.Categories
            ViewBag.CategoryId = _context.Categories // Assuming you have a Categories table
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(), // Assuming Id is the primary key of your Category model
                    Text = c.Name
                })
                .ToList();



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

            // Populate ViewBag.Brands
            ViewBag.BrandId = _context.Brands // Assuming you have a DbContext named _context
                 .Select(b => new SelectListItem
                 {
                     Value = b.Id.ToString(), // Assuming Id is the primary key of your Brand model
                     Text = b.Name
                 })
                 .ToList();

            // Populate ViewBag.Categories
            ViewBag.CategoryId = _context.Categories // Assuming you have a Categories table
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(), // Assuming Id is the primary key of your Category model
                    Text = c.Name
                })
                .ToList();

            // Pass the brand name to the view
            ViewBag.BrandName = brand.Name;

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

            // Populate ViewBag.Brands
            ViewBag.BrandId = _context.Brands // Assuming you have a DbContext named _context
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(), // Assuming Id is the primary key of your Brand model
                    Text = b.Name
                })
                .ToList();

            // Populate ViewBag.Categories
            ViewBag.CategoryId = _context.Categories // Assuming you have a Categories table
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(), // Assuming Id is the primary key of your Category model
                    Text = c.Name
                })
                .ToList();

            // Pass the brand name to the view
            ViewBag.CategoryName = category.Name;
            ViewBag.ThisCategoryId = id;

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

            return View(model);
        }

        // GET: Models/Create
        public IActionResult Create()
        {
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return PartialView("_Create");
        }

        // POST: Models/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Model model)
        {
            // Check for existing Model with the same BrandId, Name, and CategoryId
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

                // Create a log entry
                var details = $"Model {model.Name} created with Image {imageName}.";
                var myUser = User.Identity.Name ?? "Anonymous"; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser);

                // Add the log entry to the database
                await _context.SaveChangesAsync();
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
            // Fetch the brand with the specified ID
            var brand = _context.Brands.Find(brandId);

            // Check if the brand exists
            if (brand == null)
            {
                return NotFound(); // Handle case where the brand does not exist
            }

            // Create the view model
            var model = new Model
            {
                BrandId = brandId // Set the BrandId for the model
            };

            // Pass the brand name to the view
            ViewData["BrandName"] = brand.Name;

            // Populate categories as before
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");

            // Return the view
            return PartialView("_CreateByBrand", model);
        }

        // POST: Models/CreateCreateByBrand/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateByBrand(Model model)
        {
            if (ModelState.IsValid)
            {
                // Check for existing Model with the same BrandId, Name, and CategoryId
                bool exists = await _context.Models
                    .AnyAsync(m => m.BrandId == model.BrandId && m.Name == model.Name && m.CategoryId == model.CategoryId);

                if (exists)
                {
                    TempData["Failure"] = "A model with the same Brand, Name, and Category already exists.";
                    return RedirectToAction(nameof(Index));
                }

                string imageName = "noimage.png";

                if (model.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/devices");
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

                // Create a log entry using logging service
                var details = $"Model '{model.Name}' created with Image '{imageName}'.";
                var myUser = User.Identity.Name ?? "Anonymous"; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action

                TempData["Success"] = "The Model has been created Successfully!";
                return RedirectToAction(nameof(Index));
            }

            // If we got this far, something failed, redisplay form
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", model.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            return PartialView("_CreateByBrand", model);
        }


        // GET: Models/CreateByCategory/5
        public IActionResult CreateByCategory(int id)
        {
            int categoryId = id;
            // Fetch the brand with the specified ID
            var category = _context.Categories.Find(categoryId);

            // Check if the brand exists
            if (category == null)
            {
                return NotFound(); // Handle case where the brand does not exist
            }

            // Create the view model
            var model = new Model
            {
                CategoryId = categoryId // Set the BrandId for the model
            };

            // Pass the brand name to the view
            ViewData["CategoryName"] = category.Name;

            // Populate categories as before
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");

            // Return the view
            return PartialView("_CreateByCategory", model);
        }

        // POST: Models/CreateByCategory/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateByCategory(Model model)
        {
            if (ModelState.IsValid)
            {

                // Check for existing Model with the same BrandId, Name, and CategoryId
                bool exists = await _context.Models
                    .AnyAsync(m => m.BrandId == model.BrandId && m.Name == model.Name && m.CategoryId == model.CategoryId);

                if (exists)
                {
                    TempData["Failure"] = "A model with the same Brand, Name, and Category already exists.";
                    return RedirectToAction(nameof(Index));
                }

                string imageName = "noimage.png";

                if (model.ImageUpload != null)
                {
                    string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/devices");
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
                await _context.SaveChangesAsync();
                TempData["Success"] = "The Model has been created Successfully!";

                // Create a log entry using logging service
                var details = $"Model '{model.Name}' created with Image '{imageName}'.";
                var myUser = User.Identity.Name ?? "Anonymous"; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action

                return RedirectToAction(nameof(Index));
            }

            // If the model state is invalid, repopulate the dropdown lists
            ViewData["CategoryName"] = _context.Categories.Find(model.CategoryId)?.Name; // Fetch the brand name again
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", model.BrandId);
            return PartialView("_CreateByCategory", model);
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
            //return View(model);
            return PartialView("_Edit", model);
        }

        // POST: Models/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                    // Retrieve the original model
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

                    // Create a log entry
                    var log = new Log
                    {
                        Details = $"Model updated from Name: {originalModel.Name}, Image: {oldImage} to Name: {model.Name}, Image: {imageName}.",
                        User = User.Identity.Name ?? "Anonymous", // Assuming you have user authentication
                        Date = DateTime.Now
                    };

                    TempData["Success"] = "The Model has been edited Successfully!";
                    // Add the log entry to the database
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

            //return View(model);
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

                // Get the count of SerialNumbers linked to the model
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
                var myUser = User.Identity.Name ?? "Anonymous"; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action


                TempData["Success"] = "The Model has been deleted Successfully!";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Models");
        }

        private bool ModelExists(int id)
        {
            return _context.Models.Any(e => e.Id == id);
        }

        //////////////////////////MODIFICATIONS

        // Action to render the view component
        public IActionResult AllocateSerialNumber(int id)
        {
            // Get the count of serial numbers for the given model
            var count = _context.SerialNumbers.Count(sn => sn.ModelId == id);


            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name");
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Name");
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name");
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Name");

            var currentUserId = User?.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user ID
            // Get users from the AD group "ZIM-WEB-IT"
            ViewData["Users"] = _adService.GetGroupMembersSelectList("zim-web-it");

            // Pass the count to the ViewComponent
            return ViewComponent("SerialNumber", new { modelId = id, count = count });
        }

        // Action to handle form submission
        [HttpPost]
        public async Task<IActionResult> CreateSerialNumber(int modelId, string name)
        {
            // Check for existing SerialNumber with the same ModelId and Name
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
                ConditionId = 1, // Assuming 1 is a valid ConditionId
                DepartmentId = 1, // Assuming 1 is a valid DepartmentId
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

                // Create a log entry using logging service
                var details = $"Serial Number '{serialNumber.Name}' for '{brand.Name}' '{model.Name}' '{category.Name}' added.";
                var myUser = User.Identity.Name ?? "Anonymous"; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action
            }
            else
            {
                TempData["Failure"] = "Failed to add Serial Number!!!";
            }

            return RedirectToAction("AllocateSerialNumber", new { id = modelId });
        }


        [HttpPost]
        public IActionResult EditSerialNumber(SerialNumber model, string name)
        {
            int? modelId = 0;
            //if (ModelState.IsValid)
            //{
            //var serialNumber = _context.SerialNumbers.FirstOrDefault(sn => sn.Id == model.Id);


            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name");
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Name");
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name");
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Name");



            var serialNumber = _context.SerialNumbers
                                   .Include(sn => sn.Model)
                                   .Include(sn => sn.Condition) // Include Condition
                                   .Include(sn => sn.Department) // Include Department
                                   .Include(sn => sn.Location) // Include Department
                                   .Include(sn => sn.ADUsers) // Include ADUsers
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

            // Create a log entry using logging service
            var details = $"Serial Number '{serialNumber.Name}' for '{serialNumber.Model.Brand.Name}' '{serialNumber.Model.Name}' '{serialNumber.Model.Category.Name}' properties updated.";
            var myUser = User.Identity.Name ?? "Anonymous"; // Assuming you have user authentication
            _loggingService.LogActionAsync(details, myUser); // Log the action

            //return RedirectToAction("Index"); // Redirect to the main view
            return RedirectToAction("AllocateSerialNumber", new { id = modelId }); // Redirect to the same action
            
        }


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

                // Create a log entry using logging service
                var model = await _context.Models.FindAsync(serialNumber.ModelId);
                var brand = await _context.Brands.FindAsync(model.BrandId);
                var category = await _context.Categories.FindAsync(model.CategoryId);

                var details = $"Serial Number '{serialNumber.Name}' for '{brand.Name}' '{model.Name}' '{category.Name}' Deleted.";
                var myUser = User.Identity.Name ?? "Anonymous"; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action

                _context.SerialNumbers.Remove(serialNumber);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Serial Number Deleted successfully!!!";


            }
            else
            {
                TempData["Failure"] = "Failed to delete Serial Number!!!";
            }

            return RedirectToAction("AllocateSerialNumber", new { id = modelId });
        }


        //[HttpPost]
        //public async Task<IActionResult> DeleteMultipleSerialNumbers(List<int> Ids, int ModelId)
        //{
        //    if (Ids == null || Ids.Count == 0)
        //    {
        //        TempData["Failure"] = "No serial numbers selected for deletion.!!!";
        //        return BadRequest("No serial numbers selected for deletion.");
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        //var serialNumber = await _context.SerialNumbers.FindAsync(id);
        //        //if (serialNumber == null)
        //        //{
        //        //    return NotFound();
        //        //}

        //        //_context.SerialNumbers.Remove(serialNumber);
        //        //await _context.SaveChangesAsync();
        //        //TempData["Success"] = "Serial Number Deleted successfully!!!";
        //        try
        //        {
        //            var serialNumbers = _context.SerialNumbers.Where(sn => Ids.Contains(sn.Id));
        //            _context.SerialNumbers.RemoveRange(serialNumbers);
        //            await _context.SaveChangesAsync();
        //            return RedirectToAction("AllocateSerialNumber", new { id = ModelId });
        //        }
        //        catch (Exception ex)
        //        {
        //            // Log the exception (ex) as necessary
        //            return StatusCode(500, "An error occurred while deleting serial numbers.");
        //        }
        //    }
        //    return RedirectToAction("AllocateSerialNumber", new { id = ModelId });

        //}

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

    }
}



