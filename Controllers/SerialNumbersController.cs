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
using System.Security.Claims;
using Scribe.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.Elfie.Extensions;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using System.Globalization;
using System.Data;
using ExcelDataReader;
using System.ComponentModel;
using OfficeOpenXml;
using System.Text;


namespace Scribe.Controllers
{
    [Authorize(Policy = "GroupPolicy")]
    public class SerialNumbersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDeallocationService _deallocationService;
        private readonly IAllocationService _allocationService;
        private readonly ILoggingService _loggingService;
        private readonly IActiveDirectoryService _adService;

        public SerialNumbersController(ApplicationDbContext context, IDeallocationService deallocationService, IAllocationService allocationService, ILoggingService loggingService, IActiveDirectoryService adService)
        {
            _context = context;
            _deallocationService = deallocationService;
            _allocationService = allocationService;
            _loggingService = loggingService;
            _adService = adService;
        }

        // GET: SerialNumbers
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.SerialNumbers.Include(s => s.Condition).Include(s => s.Department).Include(s => s.Location).Include(s => s.Model).Include(s => s.Model.Brand).Include(s => s.ADUsers).Include(s => s.Group);
            var count = _context.SerialNumbers.Count();
            ViewData["count"] = count;
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name");
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Name");
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name");
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Name");

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Serial Numbers", Url = Url.Action("Index", "SerialNumbers"), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: SerialNumbers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serialNumber = await _context.SerialNumbers
                .Include(s => s.Condition)
                .Include(s => s.Department)
                .Include(s => s.Location)
                .Include(s => s.Model)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serialNumber == null)
            {
                return NotFound();
            }

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Serial Numbers", Url = Url.Action("Index", "SerialNumbers"), IsActive = false },
                new BreadcrumbItem { Title = serialNumber.Name, Url = Url.Action("Details", "SerialNumbers", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(serialNumber);
        }

        // GET: SerialNumbers/Create
        public IActionResult Create()
        {
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Id");
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Id");
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name");
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Name");
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Name");

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Serial Numbers", Url = Url.Action("Index", "SerialNumbers"), IsActive = false },
                new BreadcrumbItem { Title = "Create", Url = Url.Action("Create", "SerialNumbers"), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View();
        }

        // POST: SerialNumbers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ModelId,User,ConditionId,DepartmentId,LocationId,Creation,Allocation")] SerialNumber serialNumber)
        {
            if (ModelState.IsValid)
            {
                _context.Add(serialNumber);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Id", serialNumber.ConditionId);
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Id", serialNumber.DepartmentId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", serialNumber.LocationId);
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Name");
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Name", serialNumber.ModelId);

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Serial Numbers", Url = Url.Action("Index", "SerialNumbers"), IsActive = false },
                new BreadcrumbItem { Title = "Create", Url = Url.Action("Create", "SerialNumbers"), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(serialNumber);
        }

        // GET: SerialNumbers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serialNumber = await _context.SerialNumbers.FindAsync(id);
            if (serialNumber == null)
            {
                return NotFound();
            }
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name", serialNumber.ConditionId);
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Name", serialNumber.DepartmentId);
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", serialNumber.LocationId);
            ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Name");
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Name", serialNumber.ModelId);
            var model = await _context.Models.FindAsync(serialNumber.ModelId);
            var brand = await _context.Brands.FindAsync(model.BrandId);
            ViewData["Brand"] = brand.Name.ToString();
            ViewData["Users"] = new SelectList(_context.SystemUsers, "SamAccountName", "SamAccountName");

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Serial Numbers", Url = Url.Action("Index", "SerialNumbers"), IsActive = false },
                new BreadcrumbItem { Title = serialNumber.Name, Url = Url.Action("Edit", "SerialNumbers", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("_Edit", serialNumber);
        }

        // POST: SerialNumbers/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, SerialNumber serialNumber)
        //{
        //    if (id != serialNumber.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //                var existingAllocation = await _context.SerialNumberGroup
        //                    .FirstOrDefaultAsync(sng => sng.SerialNumberId == id);

        //                if (existingAllocation != null)
        //                {
        //                    //_context.Remove(existingAllocation);

        //                    var myAllocationHistory = _context.AllocationHistory.OrderByDescending(a => a.AllocationDate).First(x => x.SerialNumberId == existingAllocation.SerialNumberId);

        //                    if (myAllocationHistory != null)
        //                    {
        //                        myAllocationHistory.DeallocationDate = DateTime.Now;
        //                        _context.AllocationHistory.Update(myAllocationHistory);
        //                    }
        //                }
        //                var individualId = _context.IndividualAssignment.FirstOrDefault(x => x.ADUsersId == serialNumber.ADUsersId);
        //                var serialNumberGroup = new SerialNumberGroup();

        //                if (individualId != null)
        //                {
        //                    if (individualId.ADUsersId != 1)
        //                    {
        //                        serialNumberGroup.SerialNumberId = serialNumber.Id;
        //                        serialNumberGroup.ADUsersId = individualId.ADUsersId;
        //                    }
        //                }

        //                var allocationHistory = new AllocationHistory
        //                {
        //                    SerialNumberId = id,
        //                    ADUsersId = (int)serialNumber.ADUsersId,
        //                    AllocationDate = (DateTime)serialNumber.Allocation,
        //                    DeallocationDate = null,
        //                };
        //                _context.AllocationHistory.Add(allocationHistory);
        //                _context.SerialNumberGroup.Add(serialNumberGroup);
        //                //await _context.SaveChangesAsync();
        //                TempData["Success"] = "The Device has been reallocated";


        //            _context.Update(serialNumber);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!SerialNumberExists(serialNumber.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }

        //    ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name", serialNumber.ConditionId);
        //    ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Name", serialNumber.DepartmentId);
        //    ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name", serialNumber.LocationId);
        //    ViewData["ADUsersId"] = new SelectList(_context.ADUsers, "Id", "Name", serialNumber.ADUsersId);
        //    ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Name", serialNumber.ModelId);

        //    // Set up breadcrumbs
        //    var breadcrumbs = new List<BreadcrumbItem>
        //    {
        //        new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
        //        new BreadcrumbItem { Title = "Serial Numbers", Url = Url.Action("Index", "SerialNumbers"), IsActive = false },
        //        new BreadcrumbItem { Title = serialNumber.Name, Url = Url.Action("Edit", "SerialNumbers", new { id }), IsActive = true }
        //    };
        //    ViewData["Breadcrumbs"] = breadcrumbs;

        //    return View(serialNumber);
        //}

        [HttpPost]
        public async Task<IActionResult> Edit(SerialNumber serialNumber)
        {
            if (ModelState.IsValid)
            {
                // Fetch the last added AllocationHistory with the given SerialNumberId
                var allocationHistory = await _context.AllocationHistory
                    .Where(a => a.SerialNumberId == serialNumber.Id)
                    .OrderByDescending(a => a.Id)
                    .FirstOrDefaultAsync();

                if (allocationHistory != null)
                {
                    // Update the AllocationHistory fields
                    allocationHistory.ADUsersId = serialNumber.ADUsersId;
                    allocationHistory.GroupId = serialNumber.GroupId;
                    allocationHistory.AllocationDate = serialNumber.Allocation ?? allocationHistory.AllocationDate;
                    allocationHistory.AllocatedBy = serialNumber.AllocatedBy;

                    // Save changes to the database
                    _context.Update(allocationHistory);
                }
                else
                {
                    //ModelState.AddModelError("", "AllocationHistory not found.");
                    TempData["Failure"] = "AllocationHistory not found.";
                }


                // Fetch the SerialNumberGroup with the given SerialNumberId, ADUsersId, or GroupId
                var serialNumberGroup = await _context.SerialNumberGroup
                 .Where(s => s.SerialNumberId == serialNumber.Id &&
                 (s.ADUsersId == serialNumber.ADUsersId || s.GroupId == serialNumber.GroupId))
                 .FirstOrDefaultAsync();

                if (serialNumberGroup != null)
                {
                    // Update the SerialNumberGroup fields
                    serialNumberGroup.ADUsersId = serialNumber.ADUsersId;
                    serialNumberGroup.GroupId = serialNumber.GroupId;

                    // Save changes to the database
                    _context.Update(serialNumberGroup);
                }
               

                // Update the SerialNumber fields
                var serialNumberToUpdate = await _context.SerialNumbers.FindAsync(serialNumber.Id);
                if (serialNumberToUpdate != null)
                {
                    serialNumberToUpdate.Name = serialNumber.Name;
                    serialNumberToUpdate.ModelId = serialNumber.ModelId;
                    serialNumberToUpdate.ConditionId = serialNumber.ConditionId;
                    serialNumberToUpdate.DepartmentId = serialNumber.DepartmentId;
                    serialNumberToUpdate.LocationId = serialNumber.LocationId;
                    serialNumberToUpdate.Creation = serialNumber.Creation;
                    serialNumberToUpdate.Allocation = serialNumber.Allocation;
                    serialNumberToUpdate.AllocatedBy = serialNumber.AllocatedBy;

                    // Save changes to the database
                    _context.Update(serialNumberToUpdate);
                    TempData["Success"] = "SerialNumber updated Successfully!!";
                }
                else
                {
                    //ModelState.AddModelError("", "SerialNumber not found.");
                    TempData["Failure"] = "SerialNumber not found.";
                }

                await _context.SaveChangesAsync();
            }

            // Redirect to Index after all operations
            return RedirectToAction(nameof(Index));
        }

        // GET: SerialNumbers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serialNumber = await _context.SerialNumbers
                .Include(s => s.Condition)
                .Include(s => s.Department)
                .Include(s => s.Location)
                .Include(s => s.ADUsers)
                .Include(s => s.Model)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serialNumber == null)
            {
                return NotFound();
            }

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Serial Numbers", Url = Url.Action("Index", "SerialNumbers"), IsActive = false },
                new BreadcrumbItem { Title = serialNumber.Name, Url = Url.Action("Delete", "SerialNumbers", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("_Delete", serialNumber);
        }

        // GET: SerialNumbers/DeleteFromAllocation/5
        public async Task<IActionResult> DeleteFromAllocation(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serialNumber = await _context.SerialNumbers
                .Include(s => s.Condition)
                .Include(s => s.Department)
                .Include(s => s.Location)
                .Include(s => s.ADUsers)
                .Include(s => s.Model)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (serialNumber == null)
            {
                return NotFound();
            }

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Serial Numbers", Url = Url.Action("Index", "SerialNumbers"), IsActive = false },
                new BreadcrumbItem { Title = serialNumber.Name, Url = Url.Action("Delete", "SerialNumbers", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("_DeleteFromAllocation", serialNumber);
        }

        // POST: SerialNumbers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var serialNumber = await _context.SerialNumbers.FindAsync(id);
            if (serialNumber != null)
            {
                if (serialNumber.CurrentlyAllocated)
                {
                    TempData["Failure"] = "Device cannot be deleted as it is currently assigned.";
                    return RedirectToAction(nameof(Index));
                }

                _context.SerialNumbers.Remove(serialNumber);
                TempData["Success"] = "Device Deleted Successfully!!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: SerialNumbers/Delete/5
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedAllocation(int id)
        {
            var serialNumber = await _context.SerialNumbers.FindAsync(id);
            var modelId = serialNumber.ModelId;
            if (serialNumber != null)
            {
                if (serialNumber.CurrentlyAllocated)
                {
                    TempData["Failure"] = "Device cannot be deleted as it is currently assigned.";
                    return RedirectToAction("AllocateSerialNumber", "Models", new { id = modelId });
                    
                }

                _context.SerialNumbers.Remove(serialNumber);
                TempData["Success"] = "Device Deleted Successfully!!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("AllocateSerialNumber", "Models", new { id = modelId });

        }

        private bool SerialNumberExists(int id)
        {
            return _context.SerialNumbers.Any(e => e.Id == id);
        }

        // POST: Deallocate
        [HttpPost]
        public async Task<IActionResult> Deallocate(int allocationId, string deallocatedBy)
        {
            var allocationHistory = await _context.AllocationHistory.FindAsync(allocationId);
            try
            {
                if (deallocatedBy == null)
                {
                    deallocatedBy = User.Identity.Name;
                }
                await _deallocationService.DeallocateAsync(allocationId, deallocatedBy);
                TempData["Success"] = "Device deallocated successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Success"] = "Device deallocated successfully.";
                return RedirectToAction("ViewHistory", new { id = allocationHistory.SerialNumberId });
            }
            return RedirectToAction("ViewHistory", new { id = allocationHistory.SerialNumberId });
        }

        // GET: ViewHistory
        public async Task<IActionResult> ViewHistory(int id)
        {
            var users = await _context.ADUsers.ToListAsync();
            var modelName = _context.SerialNumbers.Include(s => s.Model).Include(s => s.Model.Category).Include(s => s.Model.Brand).Include(s => s.ADUsers).Include(s => s.Group).First(s => s.Id == id);
            var type = modelName.Model.Brand.Name + " " + modelName.Model.Name + " " + modelName.Model.Category.Name;
            ViewData["ADUsersId"] = users;
            ViewData["ModelName"] = type.ToString();
            ViewData["SerialNumber"] = modelName.Name.ToString();
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name");

            ViewData["Condition"] = _context.Condition.Where(x => x.Id == modelName.ConditionId).Select(x => x.Name).FirstOrDefault();
            ViewData["ConditionColorCode"] = _context.Condition.Where(x => x.Id == modelName.ConditionId).Select(x => x.ColorCode).FirstOrDefault();

            ViewData["Users"] = _adService.GetGroupMembersSelectList("Scribe Admins");

            var viewModel = new HistoryViewModel
            {
                SerialNumberId = id,
                Maintenance = _context.Maintenances
                    .Where(sh => sh.SerialNumberId == id)
                    .OrderByDescending(sh => sh.ServiceDate)
                    .ToList(),
                AllocationHistory = _context.AllocationHistory
                    .Where(ah => ah.SerialNumberId == id)
                    .Include(ah => ah.ADUsers)
                    .Include(ah => ah.Group)
                    .OrderByDescending(ah => ah.AllocationDate)
                    .ToList()
            };

            ViewBag.SelectedConditionId = modelName.ConditionId;

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Models", Url = Url.Action("Index", "Models"), IsActive = false },
                new BreadcrumbItem { Title = modelName.Name, Url = Url.Action("ViewHistory", "SerialNumbers", new { id }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return ViewComponent("History", new { id, model = viewModel });
        }

        // POST: CreateServiceLog
        [HttpPost]
        public async Task<IActionResult> CreateServiceLog(int SerialNumberId, string ServiceDescription, DateTime ServiceDate, DateTime NextServiceDate, int ConditionId, string SystemUserId)
        {
            if (SystemUserId == null)
            {
                SystemUserId = User.Identity.Name;
            }
            var serviceHistory = new Maintenance
            {
                SerialNumberId = SerialNumberId,
                ServiceDescription = ServiceDescription,
                ServiceDate = ServiceDate,
                NextServiceDate = NextServiceDate,
                ConditionId = ConditionId,
                SystemUserId = SystemUserId
            };

            var sn = await _context.SerialNumbers.FindAsync(SerialNumberId);
            sn.ConditionId = ConditionId;

            _context.SerialNumbers.Update(sn);
            _context.Maintenances.Add(serviceHistory);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Service Log Added";

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Serial Numbers", Url = Url.Action("Index", "SerialNumbers"), IsActive = false },
                new BreadcrumbItem { Title = sn.Name, Url = Url.Action("ViewHistory", "SerialNumbers", new { id = SerialNumberId }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return RedirectToAction("ViewHistory", new { id = serviceHistory.SerialNumberId });
        }

        // POST: RemoveService
        [HttpPost]
        public async Task<IActionResult> RemoveService(int serviceId)
        {
            var serviceHistory = await _context.Maintenances.FindAsync(serviceId);

            if (serviceHistory == null)
            {
                TempData["Failure"] = "Service Log not found.";
                return RedirectToAction("ViewHistory", new { id = serviceHistory.SerialNumberId });
            }

            _context.Maintenances.Remove(serviceHistory);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Service Log removed successfully.";

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Serial Numbers", Url = Url.Action("Index", "SerialNumbers"), IsActive = false },
                                new BreadcrumbItem { Title = serviceHistory.SerialNumber.Name, Url = Url.Action("ViewHistory", "SerialNumbers", new { id = serviceHistory.SerialNumberId }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return RedirectToAction("ViewHistory", new { id = serviceHistory.SerialNumberId });
        }

        // POST: CreateAllocation
        [HttpPost]
        public async Task<IActionResult> CreateAllocation(int SerialNumberId, int ADUsersId, DateTime AllocationDate, DateTime? DeallocationDate, string? allocatedBy)
        {
            try
            {
                if (allocatedBy == null)
                {
                    allocatedBy = User.Identity.Name;
                }
                await _allocationService.CreateAllocationAsync(SerialNumberId, ADUsersId, AllocationDate, DeallocationDate, allocatedBy);
                TempData["Success"] = "Allocation Log Added";
            }
            catch (Exception ex)
            {
                TempData["Failure"] = ex.Message;
            }

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Serial Numbers", Url = Url.Action("Index", "SerialNumbers"), IsActive = false },
                new BreadcrumbItem { Title = "View History", Url = Url.Action("ViewHistory", "SerialNumbers", new { id = SerialNumberId }), IsActive = true }
            };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return RedirectToAction("ViewHistory", new { id = SerialNumberId });
        }
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file, int modelId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please select a file.");

            var validExtensions = new[] { ".csv", ".xls", ".xlsx" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!validExtensions.Contains(fileExtension))
                return BadRequest("Invalid file format.");

            var rows = new List<UploadedRow>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                // For example: CSV with 3 columns: SerialNumber, AllocateToType, AllocateToName
                using (var reader = new StreamReader(stream))
                {
                    var header = reader.ReadLine();
                    while (!reader.EndOfStream)
                    {
                        var parts = reader.ReadLine()?.Split(',') ?? Array.Empty<string>();
                        if (parts.Length < 3) continue;

                        var row = new UploadedRow
                        {
                            SerialNumber = parts[0].Trim(),
                            AllocateToType = parts[1].Trim(),
                            AllocateToName = parts[2].Trim()
                        };

                        if (string.IsNullOrEmpty(row.SerialNumber))
                        {
                            row.IsValid = false;
                            row.Error = "Serial number is missing.";
                        }
                        else if (row.AllocateToType.Equals("User", StringComparison.OrdinalIgnoreCase))
                        {
                            var userId = _adService.GetSystemUserId(row.AllocateToName);
                            if (userId == null)
                            {
                                row.IsValid = false;
                                row.Error = $"User '{row.AllocateToName}' not found.";
                            }
                            else
                            {
                                row.ResolvedUserId = userId;
                            }
                        }
                        else if (row.AllocateToType.Equals("Group", StringComparison.OrdinalIgnoreCase))
                        {
                            var group = _context.Group.FirstOrDefault(g => g.Name == row.AllocateToName);
                            if (group == null)
                            {
                                // Auto-create group
                                group = new Group { Name = row.AllocateToName };
                                _context.Group.Add(group);
                                await _context.SaveChangesAsync();
                            }
                            row.ResolvedGroupId = group.Id;
                        }
                        else
                        {
                            row.IsValid = false;
                            row.Error = $"Unknown AllocateToType '{row.AllocateToType}'.";
                        }

                        rows.Add(row);
                    }
                }
            }

            return PartialView("_PreviewUploadRows", rows);
        }

        // POST: Confirm & Save
        [HttpPost]
        public async Task<IActionResult> SaveSerialNumbers(List<UploadedRow> rows, int modelId)
        {
            if (rows == null || !rows.Any())
                return BadRequest("Nothing to save.");

            var existingSerials = _context.SerialNumbers.Select(s => s.Name).ToHashSet();

            var toSave = new List<SerialNumber>();

            foreach (var row in rows)
            {
                if (!row.IsValid) continue;

                if (existingSerials.Contains(row.SerialNumber)) continue;

                var sn = new SerialNumber
                {
                    Name = row.SerialNumber,
                    ModelId = modelId,
                    ADUsersId = row.ResolvedUserId,
                    GroupId = row.ResolvedGroupId,
                    ConditionId = 1, // default condition
                    DepartmentId = 1,
                    LocationId = 1,
                    Creation = DateTime.Now,
                    AllocatedBy = User.Identity.Name
                };

                toSave.Add(sn);
            }

            if (!toSave.Any())
                return BadRequest("All serials already exist or none valid.");

            await _context.SerialNumbers.AddRangeAsync(toSave);
            await _context.SaveChangesAsync();

            return Ok("Upload successful.");
        }

    }
}