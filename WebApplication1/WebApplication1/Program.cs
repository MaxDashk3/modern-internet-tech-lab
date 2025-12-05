using Microsoft.EntityFrameworkCore;
using ClassLibrary1.Data;
using ClassLibrary1.Interfaces;
using ClassLibrary1.Repositories;
using ClassLibrary1.DataModels;
using WebApplication1.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using WebApplication1.Authorization;
using WebApplication1.Controllers.UniPortal.Web.Authorization;

var builder = WebApplication.CreateBuilder(args);
//var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

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

// Configure strongly typed settings
AppConfiguration appConfiguration = builder.Configuration.Get<AppConfiguration>()
    ?? throw new InvalidOperationException("Failed to bind AppConfiguration.");
builder.Services.AddSingleton(appConfiguration);


//Add services to the container.
var connectionString = appConfiguration.DefaultConnection ??
        throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IAppSqlServerRepository, AppSqlServerRepository>();

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews().AddViewLocalization().AddDataAnnotationsLocalization();

//  ��������ֲ�
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("VerifiedClient", policy =>
    {
        policy.RequireClaim("IsVerifiedClient", "True");
    });

    options.AddPolicy("CanEditResource", policy =>
        policy.AddRequirements(new IsResourceOwnerRequirement()));

    options.AddPolicy("CanEditDeveloper", policy =>
        policy.AddRequirements(new IsDeveloperOwnerRequirement()));

    //  ��˲���� ��� "������" � WorkingHours >= 100
    options.AddPolicy("PremiumOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.AddRequirements(new MinimumWorkingHoursRequirement(100));
    });

    // Custom policy for Forum
    options.AddPolicy("ForumAccessRequired", policy =>
        policy.AddRequirements(new ForumAccessRequirement()));
});

// ��������� ��������
builder.Services.AddScoped<IAuthorizationHandler, ResourceAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, DeveloperAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, MinimumWorkingHoursHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ForumAccessHandler>();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
var supportedCultures = new[] { "en-US", "uk-UA", "de-DE" };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture("en-US").AddSupportedCultures(supportedCultures).AddSupportedUICultures(supportedCultures);
});

var app = builder.Build();
app.UseRequestLocalization();
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

// �������������� �� ��� ����� ������������
app.UseAuthentication();
app.UseAuthorization();

app.UseRequestLimiter(authLimit: 100, anonLimit: 20, window: TimeSpan.FromMinutes(1));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// ��� �� ������� �� ���������� �������� �����������;
app.MapControllers().RequireAuthorization();

app.Run();
