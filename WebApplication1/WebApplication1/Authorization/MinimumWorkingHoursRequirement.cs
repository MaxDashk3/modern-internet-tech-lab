    using Microsoft.AspNetCore.Authorization;
    namespace WebApplication1.Authorization
{
        // Кастомна вимога авторизації
        public class MinimumWorkingHoursRequirement : IAuthorizationRequirement
        {
            public int MinimumHours { get; }

            public MinimumWorkingHoursRequirement(int minimumHours)
            {
                MinimumHours = minimumHours;
            }
        }
    }
