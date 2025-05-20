using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Carvisto.Data;
using Carvisto.Models;
using Carvisto.Services;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<CarvistoDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<CarvistoDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IGoogleMapsService, GoogleMapsService>();
builder.Services.AddHttpClient<IGoogleMapsService, GoogleMapsService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IReviewService, ReviewService>();

var app = builder.Build();

var cultureInfo = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"Using connection string: {connectionString}");
    
    var dataSource = connectionString?.Split(';')
        .FirstOrDefault(s => s.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
        ?.Substring("Data Source=".Length).Trim();

    Console.WriteLine($"Database path: {dataSource}");

    if (!string.IsNullOrEmpty(dataSource))
    {
        var dbDirectory = Path.GetDirectoryName(dataSource);
        Console.WriteLine($"Database directory: {dbDirectory}");
    
        if (!string.IsNullOrEmpty(dbDirectory))
        {
            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
                Console.WriteLine($"Created directory: {dbDirectory}");
            }
            else
            {
                Console.WriteLine($"Directory exists: {dbDirectory}");
                Console.WriteLine($"Directory writable: {IsDirectoryWritable(dbDirectory)}");
            }
        }
    }
    
    static bool IsDirectoryWritable(string dirPath)
    {
        try
        {
            using var fs = File.Create(
                Path.Combine(dirPath, Path.GetRandomFileName()), 
                1, 
                FileOptions.DeleteOnClose);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roleNames = { "User", "Moderator" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    var moderatorEmail = "moderator@carvisto.com";
    if (await userManager.FindByEmailAsync(moderatorEmail) == null)
    {
        var moderator = new ApplicationUser { UserName = moderatorEmail, Email = moderatorEmail, ContactName = "Moderator", ContactPhone = "123456789" };
        await userManager.CreateAsync(moderator, "Moderator123!");
        await userManager.AddToRoleAsync(moderator, "Moderator");
    }
    
}

app.Run();