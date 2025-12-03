using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using ClassLibrary1.Interfaces;
using Microsoft.Extensions.Options;
using WebApplication1.Configurations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAppSqlServerRepository _repository;
        private readonly AppConfiguration _appConfiguration;
        private readonly IStringLocalizer<HomeController> _localizer;

        public HomeController(ILogger<HomeController> logger, IAppSqlServerRepository repository,
            AppConfiguration appConfiguration, IStringLocalizer<HomeController> localizer)
        {
            _logger = logger;
            _repository = repository;
            _appConfiguration = appConfiguration;
            _localizer = localizer;
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
    }
}
