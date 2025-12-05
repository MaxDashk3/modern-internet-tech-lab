using ClassLibrary1.DataModels;
using ClassLibrary1.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using WebApplication1.Configurations;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAppSqlServerRepository _repository;
        private readonly AppConfiguration _appConfiguration;
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly UserManager<User> _userManager;

        public HomeController(ILogger<HomeController> logger, IAppSqlServerRepository repository,
            AppConfiguration appConfiguration, IStringLocalizer<HomeController> localizer, UserManager<User> userManager)
        {
            _logger = logger;
            _repository = repository;
            _appConfiguration = appConfiguration;
            _localizer = localizer;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            ViewBag.ApplicationName = _appConfiguration.AppSettings.ApplicationName;
            ViewBag.DefaultRole = _appConfiguration.AppSettings.UserSettings.DefaultRole;
            ViewBag.MaxLoginAttempts = _appConfiguration.AppSettings.UserSettings.MaxLoginAttempts;
            ViewBag.RequireEmailConfirmation = _appConfiguration.AppSettings.UserSettings.RequireEmailConfirmation;
            ViewBag.TestText = _localizer["Test"];

            var apiKey = _appConfiguration.ApiSettings.ApiKey;
            _logger.LogInformation($"Current API key: {apiKey.Substring(0, Math.Min(10, apiKey.Length))}...");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpGet("api-info")]
        public IActionResult ApiInfo()
        {
            var apiKey = _appConfiguration.ApiSettings.ApiKey;
            var maskedKey = apiKey.Length > 10
                ? apiKey.Substring(0, 10) + "..." + apiKey.Substring(apiKey.Length - 4)
                : "***";

            return Ok(new
            {
                ApiKeyPrefix = maskedKey,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                Message = "ApiKey was successfully retrieved"
            });
        }
        [HttpGet("UserId/{email}")]
        public string UserId(string email)
        {
            return _repository.GetUserByEmailAsync(email).Result?.Id ?? "User not found";
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Library(int? pageNumber, int? pageSize)
        {
            var userId = _userManager.GetUserId(User);

            var query = _repository.ReadWhere<User>(u => u.Id == userId)
                .SelectMany(u => u.Games)
                .Include(g => g.Developer)
                .OrderBy(g => g.Title); 

            var paginatedGames = await PaginatedList<Game>.CreateAsync(query, pageNumber ?? 1, pageSize ?? 6);

            return View(paginatedGames);
        }
    }
}
