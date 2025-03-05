using Scribe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scribe.Infrastructure;
using Scribe.Data;

namespace Scribe.ViewComponents
{
    public class GroupViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public GroupViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(GroupViewModel model)
        {
            //var model = new GroupViewModel
            //{
            //    Users = await _context.Users.ToListAsync(),
            //    Brands = await _context.Brands.ToListAsync()
            //};

            return View(model);
        }
    }

    public class GroupViewModel
    {
        public string? GroupName { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public ADUsers User { get; set; }
        public List<UserGroup>? Users { get; set; }
        public List<Brand>? Brands { get; set; }
        public List<Model>? Models { get; set; }
        public List<SerialNumberGroup>? SerialNumbers { get; set; }
    }

}
