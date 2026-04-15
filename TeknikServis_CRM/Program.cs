using Microsoft.EntityFrameworkCore;
using TeknikServis_CRM.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// --- 1. SERVİS KAYITLARI (SERVICES) ---

// --- 1. SERVİS KAYITLARI ---
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient(); // BU SATIRI EKLE (API istekleri için)

// DbContext Kaydı: Veritabanı bağlantısı
builder.Services.AddDbContext<CrmDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- SESSION SERVİSİ YAPILANDIRMASI ---
builder.Services.AddDistributedMemoryCache(); // Session için geçici bellek
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 dakika işlem yapılmazsa oturum düşer
    options.Cookie.HttpOnly = true; // Sadece sunucu tarafından erişilebilir (Güvenlik)
    options.Cookie.IsEssential = true; // GDPR/KVKK kuralları için zorunlu çerez
    options.Cookie.Name = ".TeknikServis.Session"; // Session çerez ismi
});

// --- AUTHENTICATION (KİMLİK DOĞRULAMA) SERVİSİ ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Index"; // Giriş sayfası
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "CRM_Auth_Cookie";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Giriş 8 saat geçerli
    });

var app = builder.Build();

// --- 2. MIDDLEWARE YAPILANDIRMASI (SIRALAMA KRİTİKTİR) ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// SIRALAMA KURALI:
app.UseAuthentication(); // 1. Kimsin?
app.UseSession();        // 2. Session'ı kullanıma aç (Authorization'dan önce olmalı!)
app.UseAuthorization();  // 3. Yetkin var mı?

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Index}/{id?}");

app.Run();