using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ClassLibrary1.Data;
using ClassLibrary1.Interfaces;
using ClassLibrary1.Repositories;
using ClassLibrary1.DataModels;
using ClassLibrary1.Configuration;
using ClassLibrary1.Services;
using WebApplication1.Configurations;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("sharedsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("appsettings.Development.json", optional: true);
    builder.Configuration.AddUserSecrets<Program>(optional: true);
}
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddJsonFile("appsettings.Production.json", optional: true);
    builder.Configuration.AddEnvironmentVariables();
}

// Configure strongly typed settings using the theoretical approach
var appConfiguration = builder.Configuration.Get<AppSettings>();
builder.Services.AddSingleton(appConfiguration);

// Add configuration service
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();

// Add services to the container.
var connectionString = appConfiguration.ConnectionStrings.DefaultConnection ?? 
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));

builder.Services.AddScoped<IAppSqlServerRepository, AppSqlServerRepository>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
