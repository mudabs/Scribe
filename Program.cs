using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Scribe.Data;
using Scribe.Infrastructure;
using Scribe.Services;
using System.Security.Claims;
using System.DirectoryServices.AccountManagement;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Use only Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("GroupPolicy", policy =>
        policy.RequireAssertion(context =>
        {
            var user = context.User;
            var domain = "zlt.co.zw";
            var groupName = "zim-web-it";

            using (var principalContext = new PrincipalContext(ContextType.Domain, domain))
            using (var group = GroupPrincipal.FindByIdentity(principalContext, groupName))
            {
                if (group != null)
                {
                    foreach (var member in group.GetMembers())
                    {
                        if (member.SamAccountName.Equals(user.Identity.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }));
    options.FallbackPolicy = options.GetPolicy("GroupPolicy");
});

// Registering Services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IDeallocationService, DeallocationService>();
builder.Services.AddScoped<IAllocationService, AllocationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddScoped<UserService>(); // Register UserService as scoped
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IActiveDirectoryService>(provider =>
{
    var context = provider.GetRequiredService<ApplicationDbContext>();
    return new ActiveDirectoryService("zlt.co.zw", "DC=zlt,DC=co,DC=zw", context);
});

// Configure Data Protection
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\keys"))
    .SetApplicationName("Scribe");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsProduction())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) =>
{
    if (!context.User.Identity.IsAuthenticated && !context.Request.Path.StartsWithSegments("/Account/Login"))
    {
        context.Response.Redirect("/Account/Login");
        return;
    }
    await next();
});

app.MapControllerRoute(
     name: "default",
     pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapRazorPages();

app.Run();