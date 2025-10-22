using WebApplication1.Configuration;

namespace WebApplication1.Configurations
{
    public class AppConfiguration
    {
        public string DefaultConnection { get; set; } = string.Empty;
        public ApiSettings ApiSettings { get; set; } = new ApiSettings();
        public AppSettings AppSettings { get; set; } = new AppSettings();
    }
}
