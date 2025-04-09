using Scribe.Infrastructure;
using Scribe.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Scribe.Services;
using Scribe.Data;

namespace Scribe.Controllers
{
    [Authorize(Policy = "GroupPolicy")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBrandService _brandService;
        private readonly ICategoryService _categoryService;
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, IBrandService brandService, ICategoryService categoryService, ILoggingService loggingService)
        {
            _context = context;
            _logger = logger;
            _brandService = brandService;
            _categoryService = categoryService;
            _loggingService = loggingService;
        }

        public async Task<IActionResult> Index()
        {
            var brands = await _brandService.GetAllBrandsAsync();
            var categories = await _categoryService.GetAllCategoriesAsync();

            var conditionData = (from s in _context.SerialNumbers
                                 group s by s.ConditionId into g
                                 select new
                                 {
                                     ConditionName = g.First().Condition.Name,
                                     Count = g.Count()
                                 }).ToList();

            ViewBag.Categories = categories;
            ViewBag.ConditionCounts = conditionData.Select(c => c.Count).ToList();
            ViewBag.ConditionNames = conditionData.Select(c => c.ConditionName).ToList();

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = true }
        };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(brands);
        }

        public async Task<IActionResult> Settings()
        {
            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "Settings", Url = Url.Action("Settings", "Home"), IsActive = true }
        };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View();
        }

        public IActionResult Exports()
        {
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name");
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name");
            ViewData["GroupId"] = new SelectList(_context.Group, "Id", "Name");

            var modelJson = HttpContext.Session.GetString("ReportModel");
            var model = modelJson == null ? null : JsonConvert.DeserializeObject<ReportModel>(modelJson);

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "Exports", Url = Url.Action("Exports", "Home"), IsActive = true }
        };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(model);
        }

        public IActionResult Export(int[] brands, int[] categories, int[] locations, int[] conditions, DateTime date, int usergroups)
        {
            var query = _context.SerialNumbers
                .Include(s => s.Model)
                    .ThenInclude(m => m.Brand)
                .Include(s => s.Model)
                    .ThenInclude(m => m.Category)
                .Include(s => s.Condition)
                .Include(s => s.Location)
                .AsQueryable();

            if (brands != null && brands.Length > 0)
            {
                query = query.Where(s => brands.Contains(s.Model.Brand.Id));
            }

            if (categories != null && categories.Length > 0)
            {
                query = query.Where(s => categories.Contains(s.Model.Category.Id));
            }

            if (locations != null && locations.Length > 0)
            {
                query = query.Where(s => locations.Contains(s.Location.Id));
            }

            if (conditions != null && conditions.Length > 0)
            {
                query = query.Where(s => conditions.Contains(s.Condition.Id));
            }

            var model = new ReportModel
            {
                Items = query.ToList()
            };

            HttpContext.Session.SetString("ReportModel", JsonConvert.SerializeObject(model));

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "Exports", Url = Url.Action("Exports", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "Export", Url = Url.Action("Export", "Home"), IsActive = true }
        };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return RedirectToAction("Exports");
        }

        [HttpGet]
        public IActionResult GetItemStatusData()
        {
            var data = _context.SerialNumbers
                .GroupBy(s => s.ConditionId)
                .Select(g => new
                {
                    ConditionID = g.Key,
                    Count = g.Count()
                })
                .ToList();

            return Json(data);
        }

        public IActionResult Privacy()
        {
            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "Privacy", Url = Url.Action("Privacy", "Home"), IsActive = true }
        };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "Error", Url = Url.Action("Error", "Home"), IsActive = true }
        };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Extract()
        {
            var modelJson = HttpContext.Session.GetString("ReportModel");
            var model = modelJson == null ? null : JsonConvert.DeserializeObject<ReportModel>(modelJson);

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "Extract", Url = Url.Action("Extract", "Home"), IsActive = true }
        };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return PartialView("_ReportPartial", model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Set up breadcrumbs
            var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem { Title = "Home", Url = Url.Action("Index", "Home"), IsActive = false },
            new BreadcrumbItem { Title = "Logout", Url = Url.Action("Logout", "Home"), IsActive = true }
        };
            ViewData["Breadcrumbs"] = breadcrumbs;

            return RedirectToAction("Login", "Access");
        }
    }

    public class ReportModel
    {
        // List of items to be included in the report
        public IList<SerialNumber> Items { get; set; } = new List<SerialNumber>();

        // File format for export (e.g., XLSX, PDF)
        public string Format { get; set; } = "XLSX";

        // Maps the format string to the corresponding GemBox SaveOptions
        //[BindNever]
        //public SaveOptions Options { get; set; }
        //public SaveOptions Options => this.FormatMappingDictionary[this.Format];

        // Dictionary to map file formats to SaveOptions
        //public IDictionary<string, SaveOptions> FormatMappingDictionary => new Dictionary<string, SaveOptions>()
        //{
        //    ["XLSX"] = new XlsxSaveOptions(),
        //    ["XLS"] = new XlsSaveOptions(),
        //    ["ODS"] = new OdsSaveOptions(),
        //    ["CSV"] = new CsvSaveOptions(CsvType.CommaDelimited),
        //    ["PDF"] = new PdfSaveOptions(),
        //    ["HTML"] = new HtmlSaveOptions() { EmbedImages = true },
        //    ["XPS"] = new XpsSaveOptions(), // XPS is supported only on Windows.
        //    ["BMP"] = new ImageSaveOptions(ImageSaveFormat.Bmp),
        //    ["PNG"] = new ImageSaveOptions(ImageSaveFormat.Png),
        //    ["JPG"] = new ImageSaveOptions(ImageSaveFormat.Jpeg),
        //    ["GIF"] = new ImageSaveOptions(ImageSaveFormat.Gif),
        //    ["TIF"] = new ImageSaveOptions(ImageSaveFormat.Tiff),
        //    ["SVG"] = new ImageSaveOptions(ImageSaveFormat.Svg)
        //};
    }
}

