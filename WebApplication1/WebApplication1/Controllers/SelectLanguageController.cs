using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace WebApplication1.Controllers
{
    public class SelectLanguageController : Controller
    {
        private readonly IOptions<RequestLocalizationOptions> LocOptions;
        public SelectLanguageController(IOptions<RequestLocalizationOptions>
        locOptions)
        {
            LocOptions = locOptions;
        }
        public IActionResult Index(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            var cultureItems = LocOptions.Value.SupportedUICultures?.ToList();
            return View(cultureItems);
        }
        [HttpPost]
        public IActionResult SetLanguage(string cultureName, string returnUrl)
        {
            Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new
            RequestCulture(cultureName)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return LocalRedirect(returnUrl);
            }
        }
    }
}
