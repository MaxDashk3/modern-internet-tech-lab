using System.Text.Json;

namespace WebApplication1.Helpers
{
    // In a Helpers folder (e.g., SessionExtensions.cs)
    public static class SessionExtensions
    {
        // Sets an object by serializing it to JSON
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Gets an object by deserializing the JSON string
        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
