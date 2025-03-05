using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Scribe.Data;
using Scribe.Infrastructure;
using Scribe.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Added Windows Authentication
//builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
//    .AddNegotiate();

//Use cookie-based authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
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
    return new ActiveDirectoryService("zlt.co.zw", "DC=zlt.co.zw,DC=com", context);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
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

app.UseAuthentication(); // Ensure this is included
app.UseAuthorization();

//Redirect unauthenticated users to the login page
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