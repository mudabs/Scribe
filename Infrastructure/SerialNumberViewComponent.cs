using Scribe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scribe.Data;

namespace Scribe.Infrastructure
{
    public class SerialNumberViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public SerialNumberViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int modelId)
        {
            //var model = await _context.Models.FindAsync(modelId);
            var model = await _context.Models.Include(m => m.Brand).FirstOrDefaultAsync(m => m.Id == modelId);
            if (model == null)
            {
                return Content("Model not found");
            }

            //var serialNumbers = await _context.SerialNumbers
            //    .Where(sn => sn.ModelId == modelId)
            //    .ToListAsync();

            var serialNumbers = await _context.SerialNumbers
               .Include(sn => sn.Condition) // Include Condition related data
               .Include(sn => sn.Department) // Include Department related data
               .Include(sn => sn.Location) // Include Location related data
               .Include(sn => sn.ADUsers) // Include AD Users related data
               .Include(sn => sn.Group) // Include AD Users related data
               .Include(sn => sn.Model) // Include Model related data
               .Where(sn => sn.ModelId == modelId)
               .ToListAsync();

            var viewModel = new SerialNumberViewModel
            {
                Model = model,
                SerialNumbers = serialNumbers,
                NumberOfDevices = serialNumbers.Count()
            };

            ViewData["modelId"] = modelId;
            return View(viewModel);
        }
    }

    public class SerialNumberViewModel
    {
        public Model Model { get; set; }
        public int NumberOfDevices { get; set; }
        public List<SerialNumber> SerialNumbers { get; set; }
    }
}