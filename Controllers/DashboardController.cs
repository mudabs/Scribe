using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scribe.Data; 
using Scribe.Models;
using System.Globalization;
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


            ViewData["Breadcrumbs"] = breadcrumbs;

            var now = DateTime.Now;

            var labels = Enumerable.Range(0, 12)
                .Select(i => now.AddMonths(-i).ToString("MMM yyyy"))
                .Reverse()
                .ToList();

            var newDevices = new List<int>();
            var allocations = new List<int>();
            var deadDevices = new List<int>();

            foreach (var label in labels)
            {
                var date = DateTime.ParseExact(label, "MMM yyyy", CultureInfo.InvariantCulture);
                var start = new DateTime(date.Year, date.Month, 1);
                var end = start.AddMonths(1);

                newDevices.Add(_context.SerialNumbers
                    .Where(s => s.Creation >= start && s.Creation < end && s.Condition.Name == "New")
                    .Count());

                allocations.Add(_context.SerialNumbers
                    .Where(s => s.Allocation >= start && s.Allocation < end && s.ADUsersId != null)
                    .Count());

                deadDevices.Add(_context.SerialNumbers
                    .Where(s => s.Condition.Name == "Dead" && s.Creation >= start && s.Creation < end)
                    .Count());
            }

            // ✅ Include SerialNumber and then its Model
            var mostServiced = _context.Maintenances
                .Include(m => m.SerialNumber)
                    .ThenInclude(sn => sn.Model)
                .Where(m => m.SerialNumber != null && m.SerialNumber.Model != null)
                .GroupBy(m => m.SerialNumber.Model.Name)
                .Select(g => new { Model = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToDictionary(x => x.Model, x => x.Count);

            // ✅ Average lifetime calculation using SerialNumbers
            var lifetimes = _context.SerialNumbers
                .Include(sn => sn.Model)
                .Where(sn => sn.Allocation != null && sn.Creation != null && sn.Model != null)
                .GroupBy(sn => sn.Model.Name)
                .Select(g => new
                {
                    Model = g.Key,
                    AvgLifetime = g.Average(sn => EF.Functions.DateDiffDay(sn.Creation.Value, sn.Allocation.Value))
                })
                .ToDictionary(x => x.Model, x => x.AvgLifetime);

            var viewModel = new DashboardViewModel
            {
                TotalDevices = _context.SerialNumbers.Count(),
                DeadDevices = _context.SerialNumbers.Count(x => x.Condition.Name == "Dead"),
                NewDevices = _context.SerialNumbers.Count(x => x.Condition.Name == "New"),
                InUseDevices = _context.SerialNumbers.Count(x => x.ADUsersId != null),

                Labels = labels,
                MonthlyNewDevices = newDevices,
                MonthlyAllocations = allocations,
                MonthlyDeadDevices = deadDevices,

                MostServicedModels = mostServiced,
                ModelLifetimes = lifetimes
            };

            return View(viewModel);
        }

        private Dictionary<string, double> ForecastModelLifetimes(Dictionary<string, double> lifetimes)
        {
            var forecasts = new Dictionary<string, double>();

            foreach (var entry in lifetimes)
            {
                // Simple forecast: +10% increase for demonstration
                double forecast = entry.Value * 1.1;
                forecasts.Add(entry.Key, forecast);
            }

            return forecasts;
        }

    }
}
