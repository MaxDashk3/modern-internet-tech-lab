using ClassLibrary1.Data;
using ClassLibrary1.DataModels;
using ClassLibrary1.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Helpers;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class GamesController : Controller
    {

        private readonly IAppSqlServerRepository _repository;
        private readonly UserManager<User> _userManager;
        private readonly IAuthorizationService _authorizationService;

        public GamesController(IAppSqlServerRepository repository, UserManager<User> userManager, IAuthorizationService authorizationService)
        {
            _repository = repository;
            _userManager = userManager;
            _authorizationService = authorizationService;
        }

        // GET: Games
        public async Task<IActionResult> Index(int? pageNumber, int? pageSize)
        {   // for disabling Add to Cart if in cart
            var sessionCart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            var cartIds = sessionCart.Select(item => item.GameId).ToList();
                
            var query = _repository.All<Game>().Include(g => g.Developer);
            var paginatedGames = await PaginatedList<Game>.CreateAsync(query, pageNumber ?? 1, pageSize ?? 6);

            // for disabling Add to Cart if in library
            var userId = _userManager.GetUserId(User);
            var ownedIds = await _repository.ReadWhere<User>(u => u.Id == userId)
            .SelectMany(u => u.Games) 
            .Select(g => g.Id)  
            .ToListAsync();

            ViewBag.OwnedGameIds = ownedIds;
            ViewBag.CartGameIds = cartIds;
            return View(paginatedGames);
        }

        // GET: Games/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var game = await _repository.All<Game>()
                .Include(g => g.Developer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game == null) return NotFound();

            // for disabling Add to Cart if in library
            var userId = _userManager.GetUserId(User);
            var owned = _repository.ReadWhere<User>(u => u.Id == userId)
            .SelectMany(u => u.Games)
            .FirstOrDefault(g => g.Id == id) != null ? true : false;
           
            // for disabling Add to Cart if in cart
            var sessionCart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            var inCart = sessionCart.FirstOrDefault( c => c.GameId == id) != null ? true : false;

            ViewBag.IsOwned = owned;
            ViewBag.IsInCart = inCart;


            return View(game);
        }

        // GET: Games/Create
        [Authorize(Policy = "VerifiedClient")]
        public IActionResult Create()
        {
            var userId = _userManager.GetUserId(User);
            ViewData["DeveloperId"] = new SelectList(_repository.ReadWhere<Developer>(d => d.AuthorId == userId), "Id", "Title");
            return View(new GameViewModel { Year = DateTime.UtcNow.Year });
        }

        // POST: Games/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "VerifiedClient")]
        public async Task<IActionResult> Create(GameViewModel model)
        {
            if (ModelState.IsValid && await IsTitleUniqueForDeveloperBool(model.Title, model.DeveloperId, model.Id))
            {
                var game = new Game
                {
                    Title = model.Title,
                    Description = model.Description,
                    ImageUrl = model.ImageUrl,
                    Year = model.Year,
                    Price = model.Price,
                    DeveloperId = model.DeveloperId
                };

                await _repository.AddAsync<Game>(game);
                return RedirectToAction(nameof(Index));
            }
            var userId = _userManager.GetUserId(User);
            ViewData["DeveloperId"] = new SelectList(_repository.ReadWhere<Developer>(d => d.AuthorId == userId), "Id", "Title");
            return View(model);
        }

        // GET: Games/Edit/5
        [Authorize(Policy = "VerifiedClient")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var game = await _repository.All<Game>()
                .Include(g => g.Developer).FirstOrDefaultAsync(g => g.Id == id);
            if (game == null) return NotFound();

            var model = new GameViewModel
            {
                Id = game.Id,
                Title = game.Title,
                Description = game.Description,
                ImageUrl = game.ImageUrl,
                Year = game.Year,
                Price = game.Price,
                DeveloperId = game.DeveloperId,
                DeveloperTitle = game.Developer?.Title
            };
            var userId = _userManager.GetUserId(User);
            ViewData["DeveloperId"] = new SelectList(_repository.ReadWhere<Developer>(d => d.AuthorId == userId), "Id", "Title");
            return View(model);
        }

        // POST: Games/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "VerifiedClient")]
        public async Task<IActionResult> Edit(int id, GameViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid && await IsTitleUniqueForDeveloperBool(model.Title, model.DeveloperId, model.Id))
            {
                var existing = await _repository.FirstOrDefaultAsync<Game>(g => g.Id == id);
                if (existing == null) return NotFound();

                existing.Title = model.Title;
                existing.Description = model.Description;
                existing.ImageUrl = model.ImageUrl;
                existing.Year = model.Year;
                existing.Price = model.Price;
                existing.DeveloperId = model.DeveloperId;

                try
                {
                    await _repository.UpdateAsync(existing);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _repository.ExistsAsync<Game>(e => e.Id == existing.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }
            var userId = _userManager.GetUserId(User);
            ViewData["DeveloperId"] = new SelectList(_repository.ReadWhere<Developer>(d => d.AuthorId == userId), "Id", "Title");
            return View(model);
        }

        // GET: Games/Delete/5
        [Authorize(Policy = "VerifiedClient")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var game = await _repository.All<Game>()
                .Include(g => g.Developer)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (game == null) return NotFound();
            var authorizationResult = await _authorizationService
                .AuthorizeAsync(User, game.Developer, "CanEditDeveloper");
            if (authorizationResult.Succeeded)
            {
                return View(game);
            }
            else if (User.Identity?.IsAuthenticated == true)
            {
                return new ForbidResult();
            }
            else
            {
                return new ChallengeResult();
            }
        }

        // POST: Games/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Policy = "VerifiedClient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _repository.FirstOrDefaultAsync<Game>(g => g.Id == id);
            if (game != null)
            {
                await _repository.RemoveAsync(game);
            }
            return RedirectToAction(nameof(Index));
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> IsTitleUniqueForDeveloper(string Title, int DeveloperId, int? Id)
        {
            if (string.IsNullOrWhiteSpace(Title))
                return Json(true);

            var excludeId = Id ?? 0;
            var exists = await _repository.ExistsAsync<Game>(g => g.Title == Title && g.DeveloperId == DeveloperId && g.Id != excludeId);

            return exists ? Json($"A game titled '{Title}' already exists for the selected developer.") : Json(true);
        }

        public async Task<bool> IsTitleUniqueForDeveloperBool(string Title, int DeveloperId, int? Id)
        {
            if (string.IsNullOrWhiteSpace(Title))
                return true;

            var excludeId = Id ?? 0;
            var exists = await _repository.ExistsAsync<Game>(g => g.Title == Title && g.DeveloperId == DeveloperId && g.Id != excludeId);

            return exists ? false: true;
        }
    }
}