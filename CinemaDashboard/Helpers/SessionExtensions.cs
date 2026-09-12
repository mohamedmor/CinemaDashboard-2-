using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace CinemaDashboard.Helpers
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T GetObjectFromJson<T>(this ISession session, string key) where T : class, new()
        {
            var value = session.GetString(key);
            return value == null ? new T() : JsonSerializer.Deserialize<T>(value);
        }
    }
}
