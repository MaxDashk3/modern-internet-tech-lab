using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ClassLibrary1.DataModels;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    public class PremiumController : Controller
    {
        private readonly UserManager<User> _userManager;

        public PremiumController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        // видаємо поточному користувачу клейм WorkingHours, це для тестування        
        [Authorize]
        public async Task<IActionResult> GrantWorkingHours()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var claims = await _userManager.GetClaimsAsync(user);
                if (!claims.Any(c => c.Type == "WorkingHours"))
                {
                    // даємо 120 годин – цього достатньо для PremiumOnly (>=100)
                    await _userManager.AddClaimAsync(user, new Claim("WorkingHours", "120"));
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // Сторінка «Преміум», доступна лише за політикою PremiumOnly
        [Authorize(Policy = "PremiumOnly")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
