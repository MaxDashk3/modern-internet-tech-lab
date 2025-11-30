namespace WebApplication1.Controllers
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using WebApplication1.Authorization;

    namespace UniPortal.Web.Authorization
    {
        public class MinimumWorkingHoursHandler
            : AuthorizationHandler<MinimumWorkingHoursRequirement>
        {
            protected override Task HandleRequirementAsync(
                AuthorizationHandlerContext context,
                MinimumWorkingHoursRequirement requirement)
            {
                // Шукаємо клейм WorkingHours
                var workingHoursClaim = context.User.Claims.FirstOrDefault(x => x.Type == "WorkingHours");
                if (workingHoursClaim is null)
                {
                    // немає клейму – доступ заборонений
                    return Task.CompletedTask;
                }

                // Пробуємо конвертувати значення клейму в число
                if (int.TryParse(workingHoursClaim.Value, out var hours))
                {
                    if (hours >= requirement.MinimumHours)
                    {
                        // Умова виконана – авторизація успішна
                        context.Succeed(requirement);
                    }
                }

                return Task.CompletedTask;
            }
        }
    }

}
