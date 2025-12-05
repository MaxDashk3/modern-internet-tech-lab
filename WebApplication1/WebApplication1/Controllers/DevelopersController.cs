using ClassLibrary1.Data;
using ClassLibrary1.DataModels;
using ClassLibrary1.Interfaces;
using ClassLibrary1.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class DevelopersController : Controller
    {
        private readonly IAppSqlServerRepository _repository;
        private readonly IAuthorizationService _authorizationService;

        public DevelopersController(IAppSqlServerRepository repository, IAuthorizationService authorizationService)
        {
            _repository = repository;
            _authorizationService = authorizationService;
        }

        // GET: Developers
        public async Task<IActionResult> Index(int? pageNumber, int? pageSize)
        {
            var query = _repository.ReadAll<Developer>();
            var paginatedDevs = await PaginatedList<Developer>.CreateAsync(query, pageNumber ?? 1, pageSize ?? 6);
            return View(paginatedDevs);
        }

        // GET: Developers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var developer = await _repository.ReadAll<Developer>()
                .Include(d => d.Games)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (developer == null) return NotFound();

            return View(developer);
        }


        // GET: Developers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Developers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeveloperViewModel model)
        {
            if (ModelState.IsValid && await IsEmailUniqueBool(model.ContactEmail, model.Id))
            {
                var developer = new Developer
                {
                    Title = model.Title,
                    Description = model.Description,
                    ContactEmail = model.ContactEmail,
                    AuthorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty
                };

                await _repository.AddAsync(developer);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Developers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var developer = await _repository.FirstOrDefaultAsync<Developer>( d => d.Id == id);
            if (developer == null) return NotFound();

            var authorizationResult = await _authorizationService
                .AuthorizeAsync(User, developer, "CanEditDeveloper");

            if (authorizationResult.Succeeded)
            {
                var model = new DeveloperViewModel
                {
                    Id = developer.Id,
                    Title = developer.Title,
                    Description = developer.Description,
                    ContactEmail = developer.ContactEmail,
                    ConfirmEmail = developer.ContactEmail
                };

                return View(model);
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

        // POST: Developers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DeveloperViewModel model)
        {
            if (id != model.Id) return NotFound();

            var existingDeveloper = await _repository.FirstOrDefaultAsync<Developer>(d => d.Id == id);
            if (existingDeveloper == null) return NotFound();

            var authorizationResult = await _authorizationService
                .AuthorizeAsync(User, existingDeveloper, "CanEditDeveloper");

            if (!authorizationResult.Succeeded)
            {
                if (User.Identity?.IsAuthenticated == true)
                {
                    return new ForbidResult();
                }
                else
                {
                    return new ChallengeResult();
                }
            }

            if (ModelState.IsValid && await IsEmailUniqueBool(model.ContactEmail, model.Id))
            {
                existingDeveloper.Title = model.Title;
                existingDeveloper.Description = model.Description;
                existingDeveloper.ContactEmail = model.ContactEmail;

                try
                {
                    await _repository.UpdateAsync(existingDeveloper);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _repository.ExistsAsync<Developer>(e => e.Id == existingDeveloper.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Developers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var developer = await _repository.FirstOrDefaultAsync<Developer>(m => m.Id == id);
            if (developer == null) return NotFound();

            var authorizationResult = await _authorizationService
                .AuthorizeAsync(User, developer, "CanEditDeveloper");

            if (authorizationResult.Succeeded)
            {
                return View(developer);
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

        // POST: Developers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var developer = await _repository.FirstOrDefaultAsync<Developer>(d => d.Id == id);
            if (developer == null) return NotFound();

            var authorizationResult = await _authorizationService
                .AuthorizeAsync(User, developer, "CanEditDeveloper");

            if (!authorizationResult.Succeeded)
            {
                if (User.Identity?.IsAuthenticated == true)
                {
                    return new ForbidResult();
                }
                else
                {
                    return new ChallengeResult();
                }
            }

            await _repository.RemoveAsync(developer);
            return RedirectToAction(nameof(Index));
        }

        // Remote validation endpoint used by DeveloperViewModel [Remote(...)]
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> IsEmailUnique(string ContactEmail, int? Id)
        {
            if (string.IsNullOrWhiteSpace(ContactEmail)) return Json(true);

            var exists = await _repository.All<Developer>()
                .AnyAsync(d => d.ContactEmail == ContactEmail && d.Id != (Id ?? 0));

            return exists ? Json($"Email {ContactEmail} is already in use.") : Json(true);
        }

        public async Task<bool> IsEmailUniqueBool(string ContactEmail, int? Id)
        {
            if (string.IsNullOrWhiteSpace(ContactEmail)) return true;

            var exists = await _repository.All<Developer>()
                .AnyAsync(d => d.ContactEmail == ContactEmail && d.Id != (Id ?? 0));

            return exists ? false : true;
        }
    }
}
