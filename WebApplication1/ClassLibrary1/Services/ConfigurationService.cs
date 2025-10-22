using ClassLibrary1.Configuration;

namespace ClassLibrary1.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly AppSettings _appSettings;

        public ConfigurationService(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public AppSettings AppSettings => _appSettings;

        public string GetApplicationName()
        {
            return _appSettings.ApplicationName;
        }

        public string GetDefaultRole()
        {
            return _appSettings.UserSettings.DefaultRole;
        }

        public string GetConnectionString()
        {
            return _appSettings.ConnectionStrings.DefaultConnection;
        }
    }
}
