using GrapheneTrace.Data;
using GrapheneTrace.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// DB CONTEXT / IDENTITY
// -------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AppDbContext>();

// -------------------------
// MVC
// -------------------------
builder.Services.AddControllersWithViews();

// -------------------------
// SESSION + CACHING
// -------------------------
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(6);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// -------------------------
// CUSTOM ANALYSIS SERVICES
// -------------------------
builder.Services.AddScoped<IPressureAnalysisService, PressureAnalysisService>();
builder.Services.AddScoped<IHeatmapService, HeatmapService>();
builder.Services.AddScoped<ITrendService, TrendService>();

var app = builder.Build();

// -------------------------
// SEED DATA (users + CSV sessions)
// -------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedData.InitializeAsync(services).GetAwaiter().GetResult();
}

// -------------------------
// MIDDLEWARE PIPELINE
// -------------------------
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

// Serve wwwroot (CSS, JS, images, etc.)
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Landing}/{action=Index}/{id?}");

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
