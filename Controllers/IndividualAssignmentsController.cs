using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Scribe.Models;
using Scribe.Services;
using Scribe.Data;

namespace Scribe.Controllers
{
    public class IndividualAssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;

        public IndividualAssignmentsController(ApplicationDbContext context,ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        // GET: IndividualAssignments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.IndividualAssignment.Include(i => i.ADUsers);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: IndividualAssignments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var individualAssignment = await _context.IndividualAssignment
                .Include(i => i.ADUsers)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (individualAssignment == null)
            {
                return NotFound();
            }

            return View(individualAssignment);
        }

        // GET: IndividualAssignments/Create
        public IActionResult Create()
        {
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Id");
            return PartialView("_Create");
        }

        // POST: IndividualAssignments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ADUsersId")] IndividualAssignment individualAssignment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(individualAssignment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Id", individualAssignment.ADUsersId);
            return PartialView("_Create");
        }

        // GET: IndividualAssignments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var individualAssignment = await _context.IndividualAssignment.FindAsync(id);
            if (individualAssignment == null)
            {
                return NotFound();
            }
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Id", individualAssignment.ADUsersId);
            return PartialView("_Edit");
        }

        // POST: IndividualAssignments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ADUsersId")] IndividualAssignment individualAssignment)
        {
            if (id != individualAssignment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(individualAssignment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IndividualAssignmentExists(individualAssignment.Id))
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
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Id", individualAssignment.ADUsersId);
            return PartialView("_Edit");
        }

        // GET: IndividualAssignments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var individualAssignment = await _context.IndividualAssignment
                .Include(i => i.ADUsers)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (individualAssignment == null)
            {
                return NotFound();
            }

            return PartialView("_Delete");
        }

        // POST: IndividualAssignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var individualAssignment = await _context.IndividualAssignment.FindAsync(id);
            if (individualAssignment != null)
            {
                _context.IndividualAssignment.Remove(individualAssignment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool IndividualAssignmentExists(int id)
        {
            return _context.IndividualAssignment.Any(e => e.Id == id);
        }


        //public async Task<IActionResult> AllocateUser(int id)
        //{
        //    // Prepare data for the view
        //    var users = await _context.ADUsers.ToListAsync();
        //    var brands = await _context.Brands.ToListAsync();

        //    ViewData["UserId"] = new SelectList(users, "Id", "Name");
        //    ViewData["BrandId"] = new SelectList(brands, "Id", "Name");

        //    // Fetch the group based on the provided id
        //    var user = await _context.IndividualAssignment.FindAsync(id); // Assuming Groups is your DbSet for the Group model

        //    // Check if the group exists
        //    if (user == null)
        //    {
        //        return NotFound(); // Handle case where the group does not exist
        //    }

        //    var viewModel = new IndividualAssignmentViewModel
        //    {
        //        IndividualAssignmentId = id,

        //    };

        //    //var userIds = await _context.UserGroup
        //    //    .Where(u => u.GroupId == id)
        //    //    .Select(ug => ug.UserId)
        //    //    .ToListAsync();

        //    //// Fetch users based on the collected user IDs
        //    //viewModel.Users = await _context.UserGroup
        //    //    .Where(u => userIds.Contains(u.UserId))
        //    //    .Include(u => u.User) // Include related User data if needed
        //    //    .ToListAsync();

        //    var serialNumberIds = await _context.SerialNumberGroup
        //        .Where(u => u.IndividualAssignmentId == id)
        //        .Include(s => s.SerialNumber.Model)
        //        .Include(s => s.SerialNumber.Model.Brand)
        //        .Include(s => s.SerialNumber.Model.Category)
        //        .Select(s => s.SerialNumberId)
        //        .ToListAsync();

        //    viewModel.SerialNumbers = await _context.SerialNumberGroup
        //        .Where(u => serialNumberIds.Contains(u.SerialNumberId))
        //        .Include(s => s.SerialNumber.Model)
        //        .Include(s => s.SerialNumber.Model.Brand)
        //        .Include(s => s.SerialNumber.Model.Category)
        //        .ToListAsync();


        //    // Pass the group model to ViewData
        //    ViewData["User"] = user;
        //    ViewData["CurrentDateTime"] = DateTime.Now;

        //    // Pass the id to the ViewComponent or directly to the view
        //    return ViewComponent("IndividualAssignment", new { id, model = viewModel });
        //}

        ////Cascading Dropdown
        //[HttpGet]
        //public JsonResult GetModelsByBrand(int brandId)
        //{
        //    var models = _context.Models.Where(m => m.BrandId == brandId)
        //                                 .Select(m => new { Id = m.Id, Name = m.Name })
        //                                 .ToList();
        //    return Json(models);
        //}

        //[HttpGet]
        //public JsonResult GetSerialNumbersByModel(int modelId)
        //{
        //    var serialNumbers = _context.SerialNumbers.Where(s => s.ModelId == modelId)
        //                                              .Select(s => new { Id = s.Id, Name = s.Name })
        //                                              .ToList();
        //    return Json(serialNumbers);
        //}

        //[HttpPost]
        //public async Task<IActionResult> CreateIndividualAllocation(int IndividualAssignmentId, int SerialNumberId)
        //{
        //    // Validate the input
        //    if (IndividualAssignmentId == 0 || SerialNumberId == 0)
        //    {
        //        ModelState.AddModelError(string.Empty, "All fields are required.");
        //        return BadRequest(ModelState);
        //    }

        //    // Check if the SerialNumber already exists in the group
        //    var existingAllocationPair = await _context.SerialNumberGroup
        //        .FirstOrDefaultAsync(sng => sng.SerialNumberId == SerialNumberId && sng.IndividualAssignmentId == IndividualAssignmentId);

        //    if (existingAllocationPair != null)
        //    {
        //        TempData["Failure"] = "The Device is already allocated to this user";
        //        return RedirectToAction("AllocateUser", new { id = IndividualAssignmentId });
        //    }

        //    // Check if the SerialNumber is currently allocated another person
        //    var existingAllocation = await _context.SerialNumberGroup
        //        .FirstOrDefaultAsync(sng => sng.SerialNumberId == SerialNumberId);

        //    if (existingAllocation != null)
        //    {
        //        //will change logic to add notification
        //        _context.Remove(existingAllocation);

        //        //Adding Deallocation Date
        //        var myAllocationHistory = _context.AllocationHistory.OrderByDescending(a => a.AllocationDate).First(x=>x.SerialNumberId == existingAllocation.SerialNumberId);
               
        //        myAllocationHistory.DeallocationDate = DateTime.Now;
        //        _context.AllocationHistory.Update(myAllocationHistory);
        //        await _context.SaveChangesAsync();
        //        TempData["Success"] = "The Device has been reallocated";
        //        //return RedirectToAction("AllocateUser", new { id = IndividualAssignmentId });
        //    }

        //    // Create a new SerialNumberGroup object
        //    var serialNumberGroup = new SerialNumberGroup
        //    {
        //        IndividualAssignmentId = IndividualAssignmentId,
        //        SerialNumberId = SerialNumberId,
        //        GroupId = null
        //    };

        //    var myUserId = _context.IndividualAssignment.Include(x => x.ADUsers).First(X => X.Id == IndividualAssignmentId);

        //    //Updating the new User's Serial Number to SerialNumbers model
        //    var mySerNumber = _context.SerialNumbers.First(x => x.Id == SerialNumberId);
        //    mySerNumber.ADUsersId = myUserId.ADUsersId;
        //    _context.Update(mySerNumber);

        //    var allocationHistory = new AllocationHistory
        //    {
        //        SerialNumberId = SerialNumberId,
        //        ADUsersId = myUserId.ADUsersId,
        //        AllocationDate = DateTime.Now,
        //        DeallocationDate = null,
        //    };

        //    // Save to the database
        //    _context.SerialNumberGroup.Add(serialNumberGroup);
        //    _context.AllocationHistory.Add(allocationHistory);
        //    await _context.SaveChangesAsync();

        //    TempData["Success"] = "Device has been allocated";
        //    // Redirect or return success response
        //    return RedirectToAction("AllocateUser", new { id = IndividualAssignmentId });
        //}

        //[HttpPost]
        //public async Task<IActionResult> RemoveDevice(int individualAllocationId)
        //{
        //    // Validate the input
        //    if (individualAllocationId == 0)
        //    {
        //        TempData["Failure"] = "Allocation not found.";
        //        return RedirectToAction("AllocateUser", new { id = individualAllocationId });
        //    }

        //    // Find the allocation to delete
        //    var allocationToDelete = await _context.SerialNumberGroup
        //.FirstOrDefaultAsync(sng => sng.IndividualAssignmentId == individualAllocationId);

        //    if (allocationToDelete == null)
        //    {
        //        TempData["Failure"] = "Allocation not found.";
        //        return RedirectToAction("AllocateUser", new { id = individualAllocationId });
        //    }

        //    //Changing the user to no user
        //    var mySerialNumber = _context.SerialNumbers.First(x=>x.Id == allocationToDelete.SerialNumberId);
        //    mySerialNumber.ADUsersId = 1;

        //    //Adding Deallocation Date
        //    var myAllocationHistory = _context.AllocationHistory.OrderByDescending(a => a.AllocationDate).First(x => x.SerialNumberId == allocationToDelete.SerialNumberId);

        //    myAllocationHistory.DeallocationDate = DateTime.Now;

        //    // Delete the allocation
        //    _context.AllocationHistory.Update(myAllocationHistory);
        //    _context.SerialNumberGroup.Remove(allocationToDelete);
        //    await _context.SaveChangesAsync();

        //    TempData["Success"] = "Allocation has been deleted successfully.";
        //    return RedirectToAction("AllocateUser", new { id = individualAllocationId });
        //}

        //[HttpGet]
        //public async Task<IActionResult> DownloadPdf(int individualAllocationId)
        //{
        //    // Find the allocation details
        //    var allocation = await _context.SerialNumberGroup
        //.FirstOrDefaultAsync(sng => sng.IndividualAssignmentId == individualAllocationId);

        //    // Create a new PDF document
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        var writer = new PdfWriter(memoryStream);
        //        var pdf = new PdfDocument(writer);
        //        var document = new Document(pdf);

        //        // Add content to the PDF
        //        document.Add(new Paragraph($"Brand: {allocation.IndividualAssignment.ADUsers.Name}"));
        //        document.Add(new Paragraph($"Brand: {allocation.SerialNumber.Model.Brand.Name}"));
        //        document.Add(new Paragraph($"Model Name: {allocation.SerialNumber.Model.Name}"));
        //        document.Add(new Paragraph($"Category: {allocation.SerialNumber.Model.Category.Name}"));
        //        document.Add(new Paragraph($"Serial Number: {allocation.SerialNumber.Name}"));

        //        document.Close();

        //        // Return the PDF as a file
        //        var fileContent = memoryStream.ToArray();
        //        return File(fileContent, "application/pdf", $"AllocationInfo {allocation.IndividualAssignment.ADUsers.Name}.pdf");
        //    }
        //}
    }
}
