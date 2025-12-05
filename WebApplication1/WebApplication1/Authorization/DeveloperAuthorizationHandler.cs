using ClassLibrary1.DataModels;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WebApplication1.Authorization
{
    public class DeveloperAuthorizationHandler : AuthorizationHandler<IsDeveloperOwnerRequirement, Developer>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            IsDeveloperOwnerRequirement requirement,
            Developer developer)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId != null && developer.AuthorId == userId)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}

