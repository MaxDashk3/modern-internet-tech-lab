using ClassLibrary1.DataModels;
using ClassLibrary1.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class ResourceController : Controller
    {
        private readonly IAppSqlServerRepository _repository;
        private readonly IAuthorizationService _authorizationService;

        public ResourceController(IAppSqlServerRepository repository, IAuthorizationService authorizationService)
        {
            _repository = repository;
            _authorizationService = authorizationService;
        }

        [Authorize(Policy = "VerifiedClient")]
        public IActionResult Index()
        {
            var resources = _repository.ReadAll<Resource>().ToList();
            return View(resources);
        }

        public async Task<IActionResult> Details(int id)
        {
            var resource = await _repository.ReadSingleAsync<Resource>(r => r.Id == id);
            
            if (resource == null)
            {
                return NotFound();
            }

            return View(resource);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Resource resource)
        {
            if (ModelState.IsValid)
            {
                resource.AuthorId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
                resource.CreatedAt = DateTime.UtcNow;
                
                await _repository.AddAsync(resource);
                return RedirectToAction(nameof(Index));
            }
            return View(resource);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var resource = await _repository.ReadSingleAsync<Resource>(r => r.Id == id);

            if (resource == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService
                .AuthorizeAsync(User, resource, "CanEditResource");

            if (authorizationResult.Succeeded)
            {
                return View(resource);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Resource resource)
        {
            if (id != resource.Id)
            {
                return NotFound();
            }

            var existingResource = await _repository.ReadSingleAsync<Resource>(r => r.Id == id);
            
            if (existingResource == null)
            {
                return NotFound();
            }

            var authorizationResult = await _authorizationService
                .AuthorizeAsync(User, existingResource, "CanEditResource");

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

            if (ModelState.IsValid)
            {
                existingResource.Title = resource.Title;
                existingResource.Content = resource.Content;
                existingResource.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(existingResource);
                return RedirectToAction(nameof(Index));
            }
            return View(resource);
        }
    }
}

