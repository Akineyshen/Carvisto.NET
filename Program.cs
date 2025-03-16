using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Carvisto.Models;

var builder = WebApplication.CreateBuilder(args);

// Настройка подключения к SQLite
var connectionString = "Data Source=app.db";
builder.Services.AddDbContext<CarvistoDbContext>(options =>
    options.UseSqlite(connectionString));

// Настройка ASP.NET Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<CarvistoDbContext>()
    .AddDefaultTokenProviders();

// Настройка маршрутизации для MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Настройка middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Добавление авторизации и аутентификации
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();