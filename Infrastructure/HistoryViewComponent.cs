using Scribe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scribe.Infrastructure;
using Scribe.ViewComponents;
using Scribe.Data;


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
        public List<ServiceHistory>? ServiceHistory { get; set; }
        public List<AllocationHistory>? AllocationHistory { get; set; }
        public List<SerialNumberGroup>? SerialNumbers { get; set; }
        public ServiceHistory NewServiceLog { get; set; } = new ServiceHistory();
        public AllocationHistory NewAllocation { get; set; } = new AllocationHistory();  
    }
}
