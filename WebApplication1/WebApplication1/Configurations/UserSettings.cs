namespace WebApplication1.Configuration
{
    public class UserSettings
    {
        public string DefaultRole { get; set; } = string.Empty;
        public int MaxLoginAttempts { get; set; } = 5;
        public bool RequireEmailConfirmation { get; set; } = true;
    }
}
