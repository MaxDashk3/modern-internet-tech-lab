using ClassLibrary1.Configuration;

namespace ClassLibrary1.Services
{
    public interface IConfigurationService
    {
        AppSettings AppSettings { get; }
        string GetApplicationName();
        string GetDefaultRole();
        string GetConnectionString();
    }
}
