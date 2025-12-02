using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Authorization
{
    public class ForumAccessHandler : AuthorizationHandler<ForumAccessRequirement>
    {
        private static readonly string[] AllowedClaims =
            { "HasForumAccess", "IsMentor", "IsVerifiedClient" };
        
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ForumAccessRequirement requirement)
        {
            var hasAccess = context.User.Claims
                .Where(c => AllowedClaims.Contains(c.Type))
                .Any(c => bool.TryParse(c.Value, out var val) && val);

            if (hasAccess)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}