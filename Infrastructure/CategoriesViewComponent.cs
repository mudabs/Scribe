using Scribe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scribe.Data;

namespace Scribe.Infrastructure
{
    public class CategoriesViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        public CategoriesViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var categories = await GetDevicesAsync();
            return View(categories);
        }

        private Task<List<Category>> GetDevicesAsync()
        {
            return _context.Categories.OrderBy(x => x.Id).ToListAsync();
        }

    }
}
