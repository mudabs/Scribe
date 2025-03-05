using Scribe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scribe.Data;

namespace Scribe.Infrastructure
{
    public class BrandsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        public BrandsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var serials = await GetBrandsAsync();
            return View(serials);
        }

        private Task<List<Brand>> GetBrandsAsync()
        {
            return _context.Brands.OrderBy(x => x.Id).ToListAsync();
        }

    }
}
