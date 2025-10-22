using WebApplication1.Configuration;

namespace WebApplication1.Configurations
{
    public class AppSettings
    {
        public string ApplicationName { get; set; } = string.Empty;
        public UserSettings UserSettings { get; set; } = new UserSettings();
    }
}
