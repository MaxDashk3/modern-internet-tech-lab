using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using ClassLibrary1.Interfaces;
using Microsoft.Extensions.Options;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAppSqlServerRepository _repository;
        private readonly ApiSettings _apiSettings;

        public HomeController(ILogger<HomeController> logger, IAppSqlServerRepository repository)
        {
            _logger = logger;
            _repository = repository;
            _apiSettings = apiSettings.Value;
        }

        public IActionResult Index()
        {
            _logger.LogInformation($"Викорситовується API ключ: {_apiSettings.ApiKey.Substring(0, Math.Min(10, _apiSettings.ApiKey.Length))}...");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpGet("api-info")]
        public IActionResult ApiInfo()
        {
            // Повернення інформації про API Key (замасковано для безпеки)
            var maskedKey = _apiSettings.ApiKey.Length > 10
                ? _apiSettings.ApiKey.Substring(0, 10) + "..." + _apiSettings.ApiKey.Substring(_apiSettings.ApiKey.Length - 4)
                : "***";

            return Ok(new
            {
                ApiKeyPrefix = maskedKey,
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                Message = "ApiKey успішно налаштовано для різних середовищ"
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
