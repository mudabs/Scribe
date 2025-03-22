using Scribe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scribe.Data;


namespace Scribe.Infrastructure
{
    public class IndividualAssignmentViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public IndividualAssignmentViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(IndividualAssignmentViewModel model)
        {
            //var model = new IndvidualAssignmentViewModel
            //{
            //    Brands = await _context.Brands.tolistasync(),
            //    Models = await _context.Models.tolistasync()
            //};

            return View(model);
        }
    }

    public class IndividualAssignmentViewModel
    {
        public int ADUsersId { get; set; }
        public ADUsers? ADUsers { get; set; }
        public List<Brand>? Brands { get; set; }
        public List<Model>? Models { get; set; }
        public List<SerialNumberGroup>? SerialNumbers { get; set; }
    }
}
