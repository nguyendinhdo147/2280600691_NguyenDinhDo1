using System.Text.Json;

namespace _2280600691_NguyenDinhDo.Extensions
{
    public static class SessionExtensions
    {
        // Phương thức để lưu đối tượng dưới dạng JSON vào Session
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "The value to set in session cannot be null.");
            }
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Phương thức để lấy đối tượng từ JSON trong Session
        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            var value = session.GetString(key);
            return string.IsNullOrEmpty(value) ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
