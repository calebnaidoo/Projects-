using GLMS.Web.Data;
using GLMS.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// ─────────────────────────────────────────────────────────────────────────────
// GLMS.Web — Combined Part 2 + Part 3
//
// Part 2:  MVC monolith with EF Core, Identity, file upload, currency API
// Part 3:  ApiService added so MVC can also demonstrate calling GLMS.API
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// ── DATABASE (Part 2) ─────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(3)));

// ── IDENTITY (Part 2) ─────────────────────────────────────────────────────────
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ── PART 2 SERVICES (Repository + Factory + Observer + File + Currency) ────────
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IServiceRequestFactory, ServiceRequestFactory>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ContractStatusLogger>();

builder.Services.AddHttpClient<ICurrencyService, CurrencyService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "GLMS-TechMove/1.0");
});

// ── PART 3: Session + HttpContextAccessor for ApiService ─────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

// ── PART 3: ApiService — typed HttpClient that calls GLMS.API ─────────────────
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://glms-backend-api:5001/";
builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

// ── AUTO-MIGRATE on startup ───────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try { db.Database.Migrate(); logger.LogInformation("✅ DB migrated."); }
    catch (Exception ex) { logger.LogError(ex, "❌ Migration failed."); }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
