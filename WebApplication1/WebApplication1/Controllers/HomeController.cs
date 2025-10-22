using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using ClassLibrary1.Interfaces;
using ClassLibrary1.Services;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAppSqlServerRepository _repository;
        private readonly IConfigurationService _configurationService;

        public HomeController(ILogger<HomeController> logger, IAppSqlServerRepository repository, IConfigurationService configurationService)
        {
            _logger = logger;
            _repository = repository;
            _configurationService = configurationService;
        }

        public IActionResult Index()
        {
            // Pass configuration data to view
            ViewBag.ApplicationName = _configurationService.GetApplicationName();
            ViewBag.DefaultRole = _configurationService.GetDefaultRole();
            ViewBag.MaxLoginAttempts = _configurationService.AppSettings.UserSettings.MaxLoginAttempts;
            ViewBag.RequireEmailConfirmation = _configurationService.AppSettings.UserSettings.RequireEmailConfirmation;
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
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
