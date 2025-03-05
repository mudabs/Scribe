using Scribe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scribe.Data;

namespace Scribe.Infrastructure
{
    public class SerialsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        public SerialsViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            string name = (string)ViewData["ModelName"];
            //var serials = await GetSerialsAsync();
            var serials = await GetSerialsByIdAsync(name);
            return View(serials);
        }

        private Task<List<SerialNumber>> GetSerialsAsync()
        {
            return _context.SerialNumbers.OrderBy(x => x.Id).ToListAsync();
        }
        private Task<List<SerialNumber>> GetSerialsByIdAsync(string? name)
        {
            return _context.SerialNumbers.Where(m => m.Model.Name == name).OrderBy(m => m.Id).ToListAsync();
        }

    }
}
