using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TeknikServis_CRM.Models;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVİS KAYITLARI ---
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<CrmDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- SESSION ---
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".TeknikServis.Session";
});

// --- AUTHENTICATION ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Index";
        options.Cookie.Name = "CRM_Auth_Cookie";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

// --- LİSANS TANIMLAMALARI ---
QuestPDF.Settings.License = LicenseType.Community;
// ClosedXML lisans satırı gerektirmez.

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Index}/{id?}");

app.Run();