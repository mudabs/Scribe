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

        public async Task<IActionResult> Index()
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
                new BreadcrumbItem { Title = "Insights", Url = Url.Action("Index", "Dashboard"), IsActive = true }
            };


            ViewData["Breadcrumbs"] = breadcrumbs;

            var model = new DashboardViewModel();

            // 1️⃣ Basic counts
            model.TotalDevices = await _context.SerialNumbers.CountAsync();
            model.DeadDevices = await _context.SerialNumbers.CountAsync(s => s.Condition.Name == "Dead");
            model.NewDevices = await _context.SerialNumbers.CountAsync(s => s.Condition.Name == "New");
            model.InUseDevices = await _context.SerialNumbers.CountAsync(s => s.Condition.Name == "In Use");

            // 2️⃣ Pie chart
            model.StatusDistribution = await _context.SerialNumbers
                .GroupBy(s => s.ConditionId)
                .Select(c => new {
                    ConditionID = g.Key,
                    Count = g.Count()
                })
                .ToDictionaryAsync(k => k.Name, v => v.Count);

            // 3️⃣ Monthly new devices
            var twelveMonthsAgo = DateTime.Now.AddMonths(-11);
            var newDevices = await _context.SerialNumbers
                .Where(s => s.Creation.HasValue && s.Creation.Value >= twelveMonthsAgo)
                .GroupBy(s => new { s.Creation.Value.Year, s.Creation.Value.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Count = g.Count()
                })
                .ToListAsync();

            model.NewDevicesMonthly = newDevices
                .OrderBy(x => x.Month)
                .ToDictionary(
                    k => k.Month.ToString("MMM yyyy"),
                    v => v.Count
                );

            //  Monthly allocations — using AllocationHistory!
            var allocationHistory = await _context.AllocationHistory
                .Where(a => a.AllocationDate >= twelveMonthsAgo)
                .GroupBy(a => new { a.AllocationDate.Year, a.AllocationDate.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Count = g.Count()
                })
                .ToListAsync();

            model.AllocationsMonthly = allocationHistory
                .OrderBy(x => x.Month)
                .ToDictionary(
                    k => k.Month.ToString("MMM yyyy"),
                    v => v.Count
                );

            // 5️⃣ Monthly dead devices trend (here using Creation as placeholder — adjust if you have another date)
            var deadDevices = await _context.SerialNumbers
                .Where(s => s.Condition.Name == "Dead" && s.Creation.HasValue && s.Creation.Value >= twelveMonthsAgo)
                .GroupBy(s => new { s.Creation.Value.Year, s.Creation.Value.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Count = g.Count()
                })
                .ToListAsync();

            model.DeviceExpiryMonthly = deadDevices
                .OrderBy(x => x.Month)
                .ToDictionary(
                    k => k.Month.ToString("MMM yyyy"),
                    v => v.Count
                );

            // 6️⃣ Most serviced models
            var servicedModels = await _context.Maintenances
                .Include(m => m.SerialNumber)
                .ThenInclude(sn => sn.Model)
                .Where(m => m.SerialNumber.Model != null)
                .GroupBy(m => m.SerialNumber.Model.Name)
                .Select(g => new
                {
                    ModelName = g.Key,
                    Count = g.Count()
                })
                .Where(x => x.Count > 1)
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            model.MostServicedModels = servicedModels
                .ToDictionary(k => k.ModelName, v => v.Count);

            // 7️⃣ Average model lifetimes
            var lifetimes = await _context.SerialNumbers
                .Include(sn => sn.Model)
                .Where(sn => sn.Model != null && sn.Creation.HasValue)
                .GroupBy(sn => sn.Model.Name)
                .Select(g => new
                {
                    ModelName = g.Key,
                    LifetimeDays = g.Average(sn => EF.Functions.DateDiffDay(sn.Creation.Value, DateTime.Now))
                })
                .ToListAsync();

            model.ModelLifetimes = lifetimes
                .ToDictionary(k => k.ModelName, v => Math.Round(v.LifetimeDays / 30.0, 1)); // Convert days to months

            // 8️⃣ Forecast = same as average for now
            model.ModelLifetimeForecasts = lifetimes
                .ToDictionary(k => k.ModelName, v => Math.Round(v.LifetimeDays / 30.0, 1));

            return View(model);
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
