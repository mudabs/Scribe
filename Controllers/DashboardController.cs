using Microsoft.AspNetCore.Mvc;
using Scribe.Data; // Adjust to your namespace for the DbContext
using Scribe.Models;
using System.Linq;

namespace Scribe.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Insights", Url = Url.Action("Index", "Dashboard"), IsActive = true }
            };
            var totalDevices = _context.SerialNumbers.Count();
            var deadDevices = _context.SerialNumbers
                .Where(x => x.Condition.Name == "Dead").Count();

            var newDevices = _context.SerialNumbers
                .Where(x => x.Condition.Name == "New").Count();

            var inUseDevices = _context.SerialNumbers
                .Where(x => x.ADUsersId != null).Count();

            var viewModel = new DashboardViewModel
            {
                TotalDevices = totalDevices,
                DeadDevices = deadDevices,
                NewDevices = newDevices,
                InUseDevices = inUseDevices
            };

            return View(viewModel);
        }
    }
}
