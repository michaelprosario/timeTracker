using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Interfaces;
using TimeTracker.Core.Services;
using TimeTracker.Infrastructure.Data;
using TimeTracker.Infrastructure.Repositories;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// Azure Key Vault Configuration (for production deployments)
// ============================================================================
if (builder.Environment.IsProduction() || builder.Environment.IsStaging())
{
    var keyVaultName = builder.Configuration["KeyVaultName"];
    if (!string.IsNullOrEmpty(keyVaultName))
    {
        var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
        
        // Use DefaultAzureCredential which supports:
        // - Managed Identity (in Azure)
        // - Azure CLI (for local development)
        // - Visual Studio, VS Code, etc.
        builder.Configuration.AddAzureKeyVault(
            keyVaultUri,
            new DefaultAzureCredential());
        
        builder.Logging.AddConsole();        
    }
}

// Add services to the container.
builder.Services.AddControllersWithViews();

// Application Insights (if configured)
var appInsightsConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
if (!string.IsNullOrEmpty(appInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry();
}

// Health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Port=5432;Database=timetracker;Username=postgres;Password=Foobar321",
        name: "postgresql",
        timeout: TimeSpan.FromSeconds(3));

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=timetracker;Username=postgres;Password=Foobar321";
builder.Services.AddDbContext<TimeTrackerDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITimeSheetRepository, TimeSheetRepository>();
builder.Services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IWorkTypeRepository, WorkTypeRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TimeSheetService>();
builder.Services.AddScoped<TimeEntryService>();
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<WorkTypeService>();

// Data Protection - configure writable directory for keys
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/tmp/dataprotection-keys"));

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

// Health check endpoint
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
