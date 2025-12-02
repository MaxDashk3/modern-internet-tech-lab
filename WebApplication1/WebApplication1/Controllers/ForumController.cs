using System.Security.Claims;
using ClassLibrary1.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class ForumController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private static readonly string[] AvailableClaims =
        { "HasForumAccess", "IsMentor", "IsVerifiedClient" };

        public ForumController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Authorize]
        public async Task<IActionResult> GrantRandomClaim()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var claims = await _userManager.GetClaimsAsync(user);

                // Remove any existing claims in the set
                foreach (var c in claims.Where(c => AvailableClaims.Contains(c.Type)))
                {
                    await _userManager.RemoveClaimAsync(user, c);
                }

                // Select a random claim
                var random = new Random();
                var selectedClaim = AvailableClaims[random.Next(AvailableClaims.Length)];

                // Add selected claim to the user's claims.
                await _userManager.AddClaimAsync(user, new Claim(selectedClaim, "true"));

                // Refresh sign-in so new claims are effective immediately
                await _signInManager.RefreshSignInAsync(user);
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = "ForumAccessRequired")]
        public ActionResult Index()
        {
            return View();
        }
    }
}

