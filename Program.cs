using ShareIT.Seeds;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ShareIT;
using ShareIT.Data;
using ShareIT.Models;
using ShareIT.Services;
using System.Globalization;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddScoped<TimelineService>();

//builder.Services.AddDefaultIdentity<Users>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

// FIXED: Combined Identity configuration - Use ONLY ONE of these approaches
// Choose Option A OR Option B, not both:

// OPTION A: Using AddIdentity (more control)
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    // Sign-in settings
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedEmail = true;

    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI(); // This adds the default Identity UI pages

// OPTION B: Using AddDefaultIdentity (simpler, if you don't need custom roles)
// builder.Services.AddDefaultIdentity<Users>(options => 
// {
//     options.SignIn.RequireConfirmedAccount = true;
// })
// .AddRoles<IdentityRole>() // Add this if you need roles
// .AddEntityFrameworkStores<ApplicationDbContext>()
// .AddDefaultTokenProviders();

// REMOVED duplicate lines:
// builder.Services.AddDefaultIdentity<Users>(...)
// builder.Services.AddIdentity<Users, IdentityRole>()

// Keep this for developer exception page
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// --- Localization (English / Arabic) ---
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(SharedResource)));

var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ar") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true; // Cookie is accessible only by server
    options.Cookie.IsEssential = true; // Mark as essential for GDPR compliance
    options.Cookie.Name = ".ShareIT.Session"; // Custom cookie name
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Use HTTPS if available
});

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerProvider>();
    var logger = loggerFactory.CreateLogger("app");

    try
    {
        var userManager = services.GetRequiredService<UserManager<Users>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Seed roles and users
        await DefaultRoles.SeedAsync(roleManager);
        await DefaultUsers.SeedBasicUserAsync(userManager);
        await DefaultUsers.SeedSuperAdminUserAsync(userManager, roleManager);

        // Seed demonstration data (companies, departments, ticket types, sample tickets)
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        await DefaultData.SeedAsync(dbContext);

        logger.LogInformation("Data seeding completed.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "An error occurred while seeding data.");
    }
}

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
app.UseSession();

// Must run before UseRouting so the culture is set before any view executes.
app.UseRequestLocalization(
    app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

app.UseRouting();

// IMPORTANT: Add authentication before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();