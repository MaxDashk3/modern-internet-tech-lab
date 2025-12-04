using ClassLibrary1.Data;
using ClassLibrary1.DataModels;
using ClassLibrary1.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class GamesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppSqlServerRepository _repository;

        public GamesController(ApplicationDbContext context, IAppSqlServerRepository repository)
        {
            _context = context;
            _repository = repository;
        }

        // GET: Games
        public async Task<IActionResult> Index(int? pageNumber, int? pageSize)
        {
            var query = _context.Games.Include(g => g.Developer);
            var paginatedGames = await PaginatedList<Game>.CreateAsync(query, pageNumber ?? 1, pageSize ?? 5);
            return View(paginatedGames);
        }

        // GET: Games/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var game = await _context.Games
                .Include(g => g.Developer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game == null) return NotFound();

            return View(game);
        }

        // GET: Games/Create
        public IActionResult Create()
        {
            ViewData["DeveloperId"] = new SelectList(_context.Developers, "Id", "Title");
            return View(new GameViewModel { Year = DateTime.UtcNow.Year });
        }

        // POST: Games/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GameViewModel model)
        {
            if (ModelState.IsValid)
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

                _context.Add(game);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["DeveloperId"] = new SelectList(_context.Developers, "Id", "Title", model.DeveloperId);
            return View(model);
        }

        // GET: Games/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var game = await _context.Games.Include(g => g.Developer).FirstOrDefaultAsync(g => g.Id == id);
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

            ViewData["DeveloperId"] = new SelectList(_context.Developers, "Id", "Title", model.DeveloperId);
            return View(model);
        }

        // POST: Games/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, GameViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _context.Games.FindAsync(id);
                if (existing == null) return NotFound();

                existing.Title = model.Title;
                existing.Description = model.Description;
                existing.ImageUrl = model.ImageUrl;
                existing.Year = model.Year;
                existing.Price = model.Price;
                existing.DeveloperId = model.DeveloperId;

                try
                {
                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Games.Any(e => e.Id == existing.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["DeveloperId"] = new SelectList(_context.Developers, "Id", "Title", model.DeveloperId);
            return View(model);
        }

        // GET: Games/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var game = await _context.Games
                .Include(g => g.Developer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (game == null) return NotFound();

            return View(game);
        }

        // POST: Games/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game != null)
            {
                _context.Games.Remove(game);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> IsTitleUniqueForDeveloper(string Title, int DeveloperId, int? Id)
        {
            if (string.IsNullOrWhiteSpace(Title))
                return Json(true);

            var excludeId = Id ?? 0;
            var exists = await _context.Games
                .AnyAsync(g => g.Title == Title && g.DeveloperId == DeveloperId && g.Id != excludeId);

            return exists ? Json($"A game titled '{Title}' already exists for the selected developer.") : Json(true);
        }
    }
}