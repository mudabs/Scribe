using Scribe.Models;
using Microsoft.AspNetCore.Mvc;


namespace Scribe.Infrastructure
{

    public class HistoryViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public HistoryViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IViewComponentResult> InvokeAsync(HistoryViewModel model)
        {
            return View(model);
        }
    }

    public class HistoryViewModel
    {
        public int SerialNumberId { get; set; }
        public List<Maintenance>? Maintenance { get; set; }
        public List<AllocationHistory>? AllocationHistory { get; set; }
        public List<SerialNumberGroup>? SerialNumbers { get; set; }
        public Maintenance NewServiceLog { get; set; } = new Maintenance();
        public AllocationHistory NewAllocation { get; set; } = new AllocationHistory();  
    }
}
