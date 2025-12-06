using ClassLibrary1.DataModels;
using ClassLibrary1.Interfaces;
using ClassLibrary1.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebApplication1.Helpers;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class CartController : Controller
    {
        private readonly IAppSqlServerRepository _repository;
        private readonly UserManager<User> _userManager;

        public CartController(IAppSqlServerRepository repository, UserManager<User> userManager)
        {
            _repository = repository;
            _userManager = userManager;
        }

        public async Task<IActionResult> AddToCart(int gameId)
        {
            var game = await _repository.FirstOrDefaultAsync<Game>(g => g.Id == gameId);
            if (game == null) return NotFound();

            // Get current cart
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();

            // CHECK: Is this game already in the cart?
            if (!cart.Any(c => c.GameId == gameId))
            {
                // Only add if it's NOT there
                cart.Add(new CartItem
                {
                    GameId = game.Id,
                    Title = game.Title,
                    Price = game.Price,
                    ImageUrl = game.ImageUrl
                });

                HttpContext.Session.Set("Cart", cart);
            }

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int id)
        {

            var cart = HttpContext.Session.Get<List<CartItem>>("Cart");

            if (cart != null)
            {
                // Find the item by ID and remove it
                cart.RemoveAll(item => item.GameId == id);

                HttpContext.Session.Set("Cart", cart);
            }

            // Redirect back
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Index()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            return View(cart);
        }

        public async Task<IActionResult> Checkout()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart");

            if (cart == null || !cart.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            var userId = _userManager.GetUserId(User);

            var user = await _repository.All<User>()
                .Include(u => u.Games)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return RedirectToAction("Login", "Account");

            var cartGameIds = cart.Select(c => c.GameId).Distinct().ToList();

            var gamesToAdd = await _repository.All<Game>()
                                              .Where(g => cartGameIds.Contains(g.Id))
                                              .ToListAsync();
            bool isModified = false;

            foreach (var game in gamesToAdd)
            {
                // Check local collection (no DB call needed here)
                if (!user.Games.Any(g => g.Id == game.Id))
                {
                    user.Games.Add(game);
                    isModified = true;
                }
            }
            if (isModified)
            {
                await _repository.SaveAsync();
            }
            HttpContext.Session.Remove("Cart");
            return View();
        }
    }
}
