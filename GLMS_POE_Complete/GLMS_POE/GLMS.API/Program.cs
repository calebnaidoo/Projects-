using GLMS.API.Data;
using GLMS.API.Repositories;
using GLMS.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;


// GLMS.API — ASP.NET Core Web API (Part 3)
// Owns the database, exposes REST endpoints, handles JWT authentication.
// The GLMS.Web MVC frontend calls this via HttpClient (zero direct DB access)


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// ── DATABASE ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(3)));

// ── IDENTITY ──────────────────────────────────────────────────────────────────
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApiDbContext>()
.AddDefaultTokenProviders();

// ── JWT AUTHENTICATION ────────────────────────────────────────────────────────
var jwtKey    = builder.Configuration["Jwt:Key"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtIssuer,
        ValidAudience            = jwtIssuer,
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// ── REPOSITORIES (Repository Pattern) ────────────────────────────────────────
builder.Services.AddScoped<IClientRepository,         ClientRepository>();
builder.Services.AddScoped<IContractRepository,       ContractRepository>();
builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();

// ── BUSINESS SERVICES (Factory + Observer) ────────────────────────────────────
builder.Services.AddScoped<IApiContractService,        ApiContractService>();
builder.Services.AddScoped<IApiServiceRequestFactory,  ApiServiceRequestFactory>();
builder.Services.AddScoped<ContractStatusLogger>();

// ── CURRENCY SERVICE ──────────────────────────────────────────────────────────
builder.Services.AddHttpClient<IApiCurrencyService, ApiCurrencyService>(c =>
{
    c.Timeout = TimeSpan.FromSeconds(15);
    c.DefaultRequestHeaders.Add("Accept", "application/json");
    c.DefaultRequestHeaders.Add("User-Agent", "GLMS-API/1.0");
});

// ── SWAGGER ───────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "GLMS API — Global Logistics Management System",
        Version     = "v1",
        Description = "REST API for TechMove Logistics. Contracts, Clients, Service Requests, Currency."
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In          = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}",
        Name        = "Authorization",
        Type        = SecuritySchemeType.ApiKey,
        Scheme      = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {{
        new OpenApiSecurityScheme { Reference = new OpenApiReference
            { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
        Array.Empty<string>()
    }});
});

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(o => o.AddPolicy("AllowFrontend", p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// ── AUTO-MIGRATE ──────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db     = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try { db.Database.Migrate(); logger.LogInformation("✅ API DB migrated."); }
    catch (Exception ex) { logger.LogError(ex, "❌ Migration failed."); }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GLMS API v1");
    c.RoutePrefix = string.Empty; // Swagger at http://localhost:5001/
});

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
