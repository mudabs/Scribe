using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scribe.Data;
using Scribe.Models;
using System.Globalization;

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
                .Where(s => s.ConditionId != null)
                .GroupBy(s => s.ConditionId)
                .Select(g => new
                {
                    ConditionId = g.Key,
                    Count = g.Count()
                })
                .Join(_context.Condition,
                      g => g.ConditionId,
                      c => c.Id,
                      (g, c) => new
                      {
                          ConditionName = c.Name,
                          Count = g.Count
                      })
                .ToDictionaryAsync(k => k.ConditionName, v => v.Count);

            // 3️⃣ Monthly new devices (12 months)
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

            // 4️⃣ Monthly allocations — using AllocationHistory
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

            // 5️⃣ Monthly dead devices trend
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

            // 7️⃣ Average lifetimes by Brand → Model
            var lifetimes = await _context.SerialNumbers
                .Include(sn => sn.Model)
                .ThenInclude(m => m.Brand)
                .Where(sn => sn.Model != null && sn.Model.Brand != null && sn.Creation.HasValue)
                .GroupBy(sn => new { BrandName = sn.Model.Brand.Name, ModelName = sn.Model.Name })
                    .Select(g => new
                    {
                        BrandName = g.Key.BrandName,
                        ModelName = g.Key.ModelName,
                        LifetimeDays = g.Average(sn => EF.Functions.DateDiffDay(sn.Creation.Value, DateTime.Now))
                    })
                .ToListAsync();

            // Group by Brand → Models for Average
            model.ModelLifetimesByBrand = lifetimes
                .GroupBy(x => x.BrandName)
                .ToDictionary(
                    g => g.Key ?? "Unknown",
                    g => g.ToDictionary(
                        m => m.ModelName ?? "Unnamed",
                        m => Math.Round(m.LifetimeDays, 2)
                    )
                );

            // 8️⃣ Forecast = same grouped shape for now
            model.ModelLifetimeForecastsByBrand = model.ModelLifetimesByBrand
                .ToDictionary(
                    b => b.Key,
                    b => b.Value.ToDictionary(
                        m => m.Key,
                        m => Math.Round(m.Value * 1.1, 2) // simple +10% for demonstration
                    )
                );

            return View(model);
        }
    }
}
