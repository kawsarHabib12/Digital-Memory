using System.Text;
using DigitalMemoryMap.BLL.Interfaces;
using DigitalMemoryMap.BLL.Services;
using DigitalMemoryMap.DAL.Data;
using DigitalMemoryMap.DAL.Interfaces;
using DigitalMemoryMap.DAL.Repositories;
using DigitalMemoryMap.Web.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Context (Microsoft SQL Server / SSMS)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=.\\SQLEXPRESS;Database=DigitalMemoryMapDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// 2. Repositories (DAL)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMemoryRepository, MemoryRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IPhotoRepository, PhotoRepository>();
builder.Services.AddScoped<IMoodRepository, MoodRepository>();

// 3. Services (BLL)
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMemoryService, MemoryService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IMoodService, MoodService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// 4. JWT Authentication with HttpOnly Cookie support
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "DigitalMemoryMap_SuperSecure_SecretKey_2026_KeyMustBeAtLeast32Chars!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "DigitalMemoryMap";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "DigitalMemoryMapUsers";
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ClockSkew = TimeSpan.Zero
    };

    // Extract JWT token from HttpOnly cookie 'dmm_token' if Authorization header is not provided
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.TryGetValue("dmm_token", out var token) && !string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// 5. Controllers and OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// 6. Global Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// 7. Auto-migration / seed execution with safe fallback
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        logger.LogInformation("Checking database connection and seeding...");
        await DbInitializer.SeedAsync(context);
        logger.LogInformation("Database ready and seeded successfully.");
    }
    catch (Exception ex)
    {
        logger.LogWarning("Database initialization skipped or deferred: {Message}. (Web server will continue to run)", ex.Message);
    }
}

// 8. HTTP Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Ensure uploads folder exists outside wwwroot
var uploadsDir = Path.Combine(app.Environment.ContentRootPath, "..", "uploads");
if (!Directory.Exists(uploadsDir))
{
    Directory.CreateDirectory(uploadsDir);
}

app.Run();
