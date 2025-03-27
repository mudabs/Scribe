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
using Scribe.Data;

namespace Scribe.Controllers
{
    public class GroupsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;
        private readonly IActiveDirectoryService _adService;

        public GroupsController(ApplicationDbContext context, ILoggingService loggingService, IActiveDirectoryService adService)
        {
            _context = context;
            _loggingService = loggingService;
            _adService = adService;
        }

        // GET: Groups
        public async Task<IActionResult> Index()
        {
            var group = await _context.Group
                .Include(s => s.SerialNumberGroups)
                .ThenInclude(s => s.SerialNumber)
                .Include(s => s.UserGroups)
                .ThenInclude(s => s.User)
                .ToListAsync();

            var viewModel = group.Select(group => new Group
            {
                Id = group.Id,
                Name = group.Name,
                UserGroups = group.UserGroups?.Select(ug => new UserGroup
                {
                    Id = ug.User.Id,
                    UserId = ug.UserId,
                    User = ug.User,
                    GroupId = ug.GroupId,
                    Group = ug.Group
                }).ToList(),
                SerialNumberGroups = group.SerialNumberGroups?.Select(sg => new SerialNumberGroup
                {
                    Id = sg.SerialNumber.Id,
                    SerialNumberId = sg.SerialNumberId,
                    SerialNumber = sg.SerialNumber,
                    GroupId = sg.GroupId,
                    Group = sg.Group
                }).ToList()
            }).ToList();
            return View(viewModel);
        }

        // GET: Groups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Group
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@group == null)
            {
                return NotFound();
            }

            return View(@group);
        }

        // GET: Groups/Create
        public IActionResult Create()
        {
            return PartialView("_Create");
        }

        // POST: Groups/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Group @group)
        {
            // Check for existing group with the same name
            bool groupExists = await _context.Group
                .AnyAsync(g => g.Name == @group.Name);

            if (groupExists)
            {
                TempData["Failure"] = "A group with the same name already exists!";
                return RedirectToAction("Create");
            }


            if (ModelState.IsValid)
            {
                _context.Add(@group);
                // Create a log entry using logging service
                var details = $"Group: {group.Name} Created.";
                var myUser = User.Identity.Name; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action
                TempData["Success"] = "New Group added successfully!!!";
                await _context.SaveChangesAsync();
                return RedirectToAction("AllocateGroup", new { id = group.Id });
            }
            else
            {
                TempData["Failure"] = "Failed to create Group!!!";
            }
            //return View(@group);
            return RedirectToAction("AllocateGroup", new { id = group.Id });
        }

        // GET: Groups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var @group = await _context.Group.FindAsync(id);
            if (@group == null)
            {
                return NotFound();
            }
            return PartialView("_Edit",group);
        }

        // POST: Groups/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Group @group)
        {
            if (id != @group.Id)
            {
                return NotFound();
            }

            // Check for existing group with the same name
            bool groupExists = await _context.Group
                .AnyAsync(g => g.Name == @group.Name);

            if (groupExists)
            {
                TempData["Failure"] = "A group with the same name already exists!";
                return RedirectToAction("Edit");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@group);
                    await _context.SaveChangesAsync();



                    // Create a log entry using logging service
                    var details = $"Group: {group.Name} Edited.";
                    var myUser = User.Identity.Name; // Assuming you have user authentication
                    await _loggingService.LogActionAsync(details, myUser); // Log the action

                    TempData["Success"] = "Group Modified successfully!!!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(@group.Id))
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
            return PartialView("_Edit", group);
        }

        // GET: Groups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Group
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@group == null)
            {
                return NotFound();
            }

            return PartialView("_Delete",group);
        }

        // POST: Groups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @group = await _context.Group.FindAsync(id);
            if (@group != null)
            {


                // Create a log entry using logging service
                var details = $"Group: {group.Name} Deleted.";
                var myUser = User.Identity.Name; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action

                _context.Group.Remove(@group);

                TempData["Failure"] = "Group Deleted successfully!!!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Groups/RemoveDevice/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveDevice(int id)
        {
            var @group = await _context.SerialNumberGroup.Include(x=>x.SerialNumber.Model).Include(x=>x.SerialNumber.Model.Brand).Include(x=>x.SerialNumber.Model.Category).FirstOrDefaultAsync(x=>x.Id == id);
            var groupId = @group?.GroupId;
            if (@group != null)
            {
                // Create a log entry using logging service
                var details = $"Device: {group.SerialNumber.Name} Removed from Group.";
                var myUser = User.Identity.Name; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action

                _context.SerialNumberGroup.Remove(@group);
                TempData["Success"] = "Device removed successfully!!!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("AllocateGroup", new { id = groupId });
        }

        // POST: Groups/RemoveUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUser(int id)
        {
            var user = await _context.UserGroup.FindAsync(id);
            var groupId = user?.GroupId;
            var groupName = _context.Group.FirstOrDefault(x=>x.Id == groupId);
            if (user != null)
            {
                // Create a log entry using logging service
                var details = $"User: {user.User} removed from Group {groupName.Name}.";
                var myUser = User.Identity.Name; // Assuming you have user authentication
                await _loggingService.LogActionAsync(details, myUser); // Log the action

                _context.UserGroup.Remove(user);
                TempData["Success"] = "User Removed successfully!!!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("AllocateGroup", new { id = groupId });
        }

        private bool GroupExists(int id)
        {
            return _context.Group.Any(e => e.Id == id);
        }

        public async Task<IActionResult> AllocateGroup(int id)
        {
            // Prepare data for the view
            var users = await _context.ADUsers.ToListAsync();
            var brands = await _context.Brands.ToListAsync();

            ViewData["UserId"] = new SelectList(users, "Id", "Name");
            ViewData["BrandId"] = new SelectList(brands, "Id", "Name");

            // Fetch the group based on the provided id
            var group = await _context.Group.FindAsync(id); // Assuming Groups is your DbSet for the Group model

            // Check if the group exists
            if (group == null)
            {
                return NotFound(); // Handle case where the group does not exist
            }

            var viewModel = new GroupViewModel
            {
                GroupName = group.Name,
                GroupId = id,
                Users = new List<UserGroup>(), // Initialize Users list
                SerialNumbers = new List<SerialNumberGroup>() // Initialize SerialNumbers list
            };

            var userIds = await _context.UserGroup
                .Where(u => u.GroupId == id)
                .Select(ug => ug.UserId)
                .ToListAsync();

            // Fetch users based on the collected user IDs
            viewModel.Users = await _context.UserGroup
                .Where(u => userIds.Contains(u.UserId))
                .Include(u => u.User) // Include related User data if needed
                .ToListAsync();

            var serialNumberIds = await _context.SerialNumberGroup
                .Where(u => u.GroupId == id)
                .Include(s => s.SerialNumber.Model)
                .Include(s => s.SerialNumber.Model.Brand)
                .Select(s => s.SerialNumberId)
                .ToListAsync();

            viewModel.SerialNumbers = await _context.SerialNumberGroup
                .Where(u => serialNumberIds.Contains(u.SerialNumberId))
                .Include(s => s.SerialNumber.Model)
                .Include(s => s.SerialNumber.Model.Brand)
                .ToListAsync();

            // Pass the group model to ViewData
            ViewData["Group"] = group;

            // Pass the id to the ViewComponent or directly to the view
            return ViewComponent("Group", new { id, model = viewModel });
        }



        [HttpPost]
        public async Task<IActionResult> CreateGroup(GroupViewModel model, List<int> selectedUserIds, List<int> selectedSerialNumberIds)
        {
            if (ModelState.IsValid)
            {
                // Create the group
                var group = new Group
                {
                    Name = model.GroupName,
                    UserGroups = selectedUserIds.Select(userId => new UserGroup { UserId = userId }).ToList(),
                    SerialNumberGroups = selectedSerialNumberIds.Select(serialId => new SerialNumberGroup { SerialNumberId = serialId }).ToList()
                };

                _context.Group.Add(group);
                await _context.SaveChangesAsync();

                // Log entry creation
                var userNames = await _context.ADUsers
                    .Where(u => selectedUserIds.Contains(u.Id))
                    .Select(u => u.Name) // Assuming there's a Name property
                    .ToListAsync();

                var serialNumbers = await _context.SerialNumbers
                    .Where(s => selectedSerialNumberIds.Contains(s.Id))
                    .Select(s => s.SerialNumbers) // Assuming there's a SerialNumber property
                    .ToListAsync();

                // Create a log entry
                var logDetails = $"Group '{group.Name}' created with users: {string.Join(", ", userNames)} and serial numbers: {string.Join(", ", serialNumbers)}.";
                var user = User.Identity.Name;
                await _loggingService.LogActionAsync(logDetails, user); // Assuming you have a logging service

                TempData["Success"] = "New Group Created successfully!!!";

                // Clear the form inputs
                ModelState.Clear();
                model.GroupName = string.Empty;
            }

            // Reload necessary data for the AllocateGroup view
            model.Users = await _context.UserGroup.ToListAsync();
            model.Brands = await _context.Brands.ToListAsync();

            // Return to the AllocateGroup view
            return View("AllocateGroup", model);
        }

        // POST: UserGroups/CreateSerialNumberGroups
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSerialNumberGroups(SerialNumberGroup serialNumberGroup)
        {
            // Repopulate dropdowns for the view component

            // Get users from the AD group "ZIM-WEB-IT"
            ViewData["UserId"] = _adService.GetGroupMembersSelectList("zim-web-it");
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");

            // Check for existing UserGroup with the same UserId and GroupId
            bool exists = await _context.SerialNumberGroup
                .AnyAsync(ug => ug.SerialNumberId == serialNumberGroup.SerialNumberId && ug.GroupId == serialNumberGroup.GroupId);

            bool existsInIndividual = await _context.SerialNumberGroup
                .AnyAsync(ug => ug.SerialNumberId == serialNumberGroup.SerialNumberId);

            
            if (existsInIndividual)
            {
                TempData["Failure"] = "The Device is already assigned a user !!!";
                if (exists)
                {
                    TempData["Failure"] = "The Device already exists in the group !!!";
                }
                return RedirectToAction("AllocateGroup", new { id = serialNumberGroup.GroupId });
            }

            serialNumberGroup.ADUsersId = null;
            var sn = _context.SerialNumbers.Include(x=>x.Model).Include(x=>x.Model.Brand).Include(x=>x.Model.Category).FirstOrDefault(X=>X.Id == serialNumberGroup.SerialNumberId);

            if (ModelState.IsValid)
            {
                await _loggingService.LogActionAsync($"Device: {sn.Model.Brand.Name} {sn.Model.Name} {sn.Name} added to Group: {serialNumberGroup.GroupId}.",User.Identity.Name);


                _context.SerialNumberGroup.Add(serialNumberGroup);
                await _context.SaveChangesAsync();



                TempData["Success"] = "New Device added successfully!!!";
                // Redirect to the AllocateGroup action with the group ID
                return RedirectToAction("AllocateGroup", new { id = serialNumberGroup.GroupId });
            }

            return RedirectToAction("AllocateGroup", new { id = serialNumberGroup.GroupId });
        }


        // POST: UserGroups/CreateUserGroups
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUserGroups(UserGroup userGroup)
        {
            // Get users from the AD group "ZIM-WEB-IT"
            ViewData["UserId"] = _adService.GetGroupMembersSelectList("zim-web-it");

            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");

            // Check for existing UserGroup with the same UserId and GroupId
            bool exists = await _context.UserGroup
                .AnyAsync(ug => ug.UserId == userGroup.UserId && ug.GroupId == userGroup.GroupId);

            if (exists)
            {
                TempData["Failure"] = "The User already exists in the group !!!";
                return RedirectToAction("AllocateGroup", new { id = userGroup.GroupId });
            }

            if (ModelState.IsValid)
            {
                _context.UserGroup.Add(userGroup);
                await _context.SaveChangesAsync();

                var user = await _context.ADUsers.FirstOrDefaultAsync(x => x.Id == userGroup.UserId);
                var groupName = _context.Group.FindAsync(userGroup.GroupId);
                await _loggingService.LogActionAsync($"Employee: {user.Name} added to Group: {groupName}.", User.Identity.Name);

                TempData["Success"] = "New User added successfully!!!";
                // Redirect to the AllocateGroup action with the group ID
                return RedirectToAction("AllocateGroup", new { id = userGroup.GroupId });
            }

            // If we reach here, something failed; re-display the form
            return View(userGroup);
        }

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


    public class GroupsViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<ADUsers>? Users { get; set; }
        public List<SerialNumber>? SerialNumbers { get; set; }
    }
}
