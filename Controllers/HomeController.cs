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

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger, IBrandService brandService, ICategoryService categoryService,ILoggingService loggingService)
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
            ViewBag.Categories = categories;
            return View(brands);
        }
        public async Task<IActionResult> Settings()
        {
            //string groupName = "zim-web-it";
            //string domain = "zlt.co.zw";

            //List<UserInfo> users = GetUsersInGroup(domain, groupName);

            
            return View();
        }

        //public static List<UserInfo> GetUsersInGroup(string domain, string groupName)
        //{
        //    var users = new List<UserInfo>();

        //    using (var context = new PrincipalContext(ContextType.Domain, domain))
        //    using (var group = GroupPrincipal.FindByIdentity(context, groupName))
        //    {
        //        if (group != null)
        //        {
        //            foreach (var principal in group.GetMembers())
        //            {
        //                DirectoryEntry de = principal.GetUnderlyingObject() as DirectoryEntry;
        //                if (de != null)
        //                {
        //                    users.Add(new UserInfo
        //                    {
        //                        FirstName = de.Properties["givenName"].Value?.ToString(),
        //                        LastName = de.Properties["sn"].Value?.ToString(),
        //                        SamAccountName = de.Properties["samAccountName"].Value?.ToString(),
        //                        UserPrincipalName = de.Properties["userPrincipalName"].Value?.ToString()
        //                    });
        //                }
        //            }
        //        }
        //    }

        //    return users;
        //}

      

        public IActionResult Exports()
        {

            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["ModelId"] = new SelectList(_context.Models, "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["LocationId"] = new SelectList(_context.Locations, "Id", "Name");
            ViewData["ConditionId"] = new SelectList(_context.Condition, "Id", "Name");
            ViewData["GroupId"] = new SelectList(_context.Group, "Id", "Name");


            // Retrieve the model from Session
            var modelJson = HttpContext.Session.GetString("ReportModel");
            var model = modelJson == null ? null : JsonConvert.DeserializeObject<ReportModel>(modelJson);

            return View(model);
        }

        public IActionResult Export(int[] brands, int[] categories, int[] locations, int[] conditions, DateTime date, int usergroups)
        {
            // Begin with the base query
            var query = _context.SerialNumbers
                .Include(s => s.Model)
                    .ThenInclude(m => m.Brand)
                .Include(s => s.Model)
                    .ThenInclude(m => m.Category)
                .Include(s => s.Condition)
                .Include(s => s.Location)
                .AsQueryable(); // Enable further filtering

            


            // Apply filters only if the corresponding parameter is not null or empty
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

            //if (date.HasValue)
            //{
            //    query = query.Where(s => s.DateAdded.Date == date.Value.Date); // Compare only the date part
            //}

            //if (!string.IsNullOrEmpty(usergroups))
            //{
            //    query = query.Where(s => s.UserGroup == usergroups);
            //}

            // Fetch the filtered data
            var model = new ReportModel
            {
                Items = query.ToList() // Execute the query and load the results
            };

            // Store the model in Session as JSON
            HttpContext.Session.SetString("ReportModel", JsonConvert.SerializeObject(model));
            string key = "ReportModel";

            //return RedirectToAction("Extract", "Home", key);
            return RedirectToAction("Exports");

        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        public IActionResult Extract()
        {
            // Retrieve the model from Session
            var modelJson = HttpContext.Session.GetString("ReportModel");
            var model = modelJson == null ? null : JsonConvert.DeserializeObject<ReportModel>(modelJson);

            return PartialView("_ReportPartial",model);
        }
                public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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

