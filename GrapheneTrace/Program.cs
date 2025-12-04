using GrapheneTrace.Data;
using GrapheneTrace.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// DB CONTEXT
// -------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// -------------------------
// MVC
// -------------------------
builder.Services.AddControllersWithViews();

// -------------------------
// SESSION
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
// CUSTOM SERVICES
// -------------------------
builder.Services.AddScoped<IPressureAnalysisService, PressureAnalysisService>();
builder.Services.AddScoped<IHeatmapService, HeatmapService>();
builder.Services.AddScoped<ITrendService, TrendService>();

var app = builder.Build();

// -------------------------
// SEED DATA
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
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Landing}/{action=Index}/{id?}");

app.Run();
