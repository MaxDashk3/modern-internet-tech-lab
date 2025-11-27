using ClassLibrary1.DataModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WebApplication1.Authorization
{
    public class ResourceAuthorizationHandler : AuthorizationHandler<IsResourceOwnerRequirement, Resource>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            IsResourceOwnerRequirement requirement,
            Resource resource)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null && resource.AuthorId == userId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}

