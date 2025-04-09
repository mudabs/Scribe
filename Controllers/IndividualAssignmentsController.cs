using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Scribe.Infrastructure;
using Scribe.Models;
using Scribe.Services;

namespace Scribe.Controllers
{
    public class IndividualAssignmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;

        public IndividualAssignmentsController(ApplicationDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        // GET: IndividualAssignments
        public async Task<IActionResult> Index()
        {
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "Individual Assignments", Url = Url.Action("Index", "IndividualAssignments"), IsActive = true }
        };
            ViewData["Breadcrumbs"] = breadcrumbs;

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

            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "Individual Assignments", Url = Url.Action("Index", "IndividualAssignments"), IsActive = false },
            new BreadcrumbItem { Title = "Details", Url = Url.Action("Details", "IndividualAssignments", new { id }), IsActive = true }
        };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(individualAssignment);
        }

        // GET: IndividualAssignments/Create
        public IActionResult Create()
        {
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "Individual Assignments", Url = Url.Action("Index", "IndividualAssignments"), IsActive = false },
            new BreadcrumbItem { Title = "Create", Url = Url.Action("Create", "IndividualAssignments"), IsActive = true }
        };
            ViewData["Breadcrumbs"] = breadcrumbs;

            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Name");
            return PartialView("_Create");
        }

        // POST: IndividualAssignments/Create
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

            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "Individual Assignments", Url = Url.Action("Index", "IndividualAssignments"), IsActive = false },
            new BreadcrumbItem { Title = "Edit", Url = Url.Action("Edit", "IndividualAssignments", new { id }), IsActive = true }
        };
            ViewData["Breadcrumbs"] = breadcrumbs;

            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Id", individualAssignment.ADUsersId);
            return PartialView("_Edit");
        }

        // POST: IndividualAssignments/Edit/5
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

            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "Individual Assignments", Url = Url.Action("Index", "IndividualAssignments"), IsActive = false },
            new BreadcrumbItem { Title = "Delete", Url = Url.Action("Delete", "IndividualAssignments", new { id }), IsActive = true }
        };
            ViewData["Breadcrumbs"] = breadcrumbs;

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

        //Individual Allocations Logic
        //Allocating a device to a user
        public async Task<IActionResult> AllocateUser(int id)
        {
            // Prepare data for the view
            var users = await _context.ADUsers.ToListAsync();
            var brands = await _context.Brands.ToListAsync();

            ViewData["UserId"] = new SelectList(users, "Id", "Name");
            ViewData["BrandId"] = new SelectList(brands, "Id", "Name");

            // Fetch the group based on the provided id
            var user = await _context.ADUsers.FindAsync(id); // Assuming Groups is your DbSet for the Group model

            // Check if the group exists
            if (user == null)
            {
                return NotFound(); // Handle case where the group does not exist
            }

            var viewModel = new IndividualAssignmentViewModel
            {
                ADUsersId = id,

            };

            var serialNumberIds = await _context.SerialNumberGroup
                .Where(u => u.ADUsersId == id)
                .Include(s => s.SerialNumber.Model)
                .Include(s => s.SerialNumber.Model.Brand)
                .Include(s => s.SerialNumber.Model.Category)
                .Select(s => s.SerialNumberId)
                .ToListAsync();

            viewModel.SerialNumbers = await _context.SerialNumberGroup
                .Where(u => serialNumberIds.Contains(u.SerialNumberId))
                .Include(s => s.SerialNumber.Model)
                .Include(s => s.SerialNumber.Model.Brand)
                .Include(s => s.SerialNumber.Model.Category)
                .ToListAsync();


            // Pass the group model to ViewData
            ViewData["User"] = user;
            ViewData["CurrentDateTime"] = DateTime.Now;

            // Pass the id to the ViewComponent or directly to the view
            return ViewComponent("IndividualAssignment", new { id, model = viewModel });
        }

        ////Cascading Dropdown to Allocate Group
        [HttpGet]
        public JsonResult GetModelsByBrand(int brandId)
        {
            var models = _context.Models.Where(m => m.BrandId == brandId)
                                         .Select(m => new { Id = m.Id, Name = m.Name })
                                         .ToList();
            return Json(models);
        }


        [HttpGet]
        public JsonResult GetSerialNumbersByModel(int modelId)
        {
            var serialNumbers = _context.SerialNumbers
                                        .Where(s => s.ModelId == modelId && !_context.SerialNumberGroup.Any(g => g.SerialNumberId == s.Id))
                                        .Select(s => new { Id = s.Id, Name = s.Name })
                                        .ToList();

            return Json(serialNumbers);
        }

    }
}
