namespace ClassLibrary1.Configuration
{
    public class AppSettings
    {
        public string ApplicationName { get; set; } = string.Empty;
        public UserSettings UserSettings { get; set; } = new UserSettings();
        public ConnectionStrings ConnectionStrings { get; set; } = new ConnectionStrings();
    }
}
