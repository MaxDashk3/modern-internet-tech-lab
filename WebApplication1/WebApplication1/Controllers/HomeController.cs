using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using ClassLibrary1.Interfaces;
using ClassLibrary1.Services;
using Microsoft.Extensions.Options;
using WebApplication1.Configurations;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAppSqlServerRepository _repository;
        private readonly ApiSettings _apiSettings;
        private readonly IConfigurationService _configurationService;

        public HomeController(ILogger<HomeController> logger, IAppSqlServerRepository repository, IConfigurationService configurationService, IOptions<ApiSettings> apiSettings)
        {
            _logger = logger;
            _repository = repository;
            _configurationService = configurationService;
            _apiSettings = apiSettings.Value;
        }

        public IActionResult Index()
        {
            // Pass configuration data to view
            ViewBag.ApplicationName = _configurationService.GetApplicationName();
            ViewBag.DefaultRole = _configurationService.GetDefaultRole();
            ViewBag.MaxLoginAttempts = _configurationService.AppSettings.UserSettings.MaxLoginAttempts;
            ViewBag.RequireEmailConfirmation = _configurationService.AppSettings.UserSettings.RequireEmailConfirmation;
            
            _logger.LogInformation($"��������������� API ����: {_apiSettings.ApiKey.Substring(0, Math.Min(10, _apiSettings.ApiKey.Length))}...");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpGet("api-info")]
        public IActionResult ApiInfo()
        {
            // ���������� ���������� ��� API Key (����������� ��� �������)
            var maskedKey = _apiSettings.ApiKey.Length > 10
                ? _apiSettings.ApiKey.Substring(0, 10) + "..." + _apiSettings.ApiKey.Substring(_apiSettings.ApiKey.Length - 4)
                : "***";

            return Ok(new
            {
                ApiKeyPrefix = maskedKey,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                Message = "ApiKey ������ ����������� ��� ����� ���������"
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
