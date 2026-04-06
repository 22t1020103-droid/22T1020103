using System.Text.Json;

namespace SV22T1020103.Shop // Đảm bảo namespace này giống các file khác của bạn
{
    public static class SessionExtensions
    {
        // 1. Hàm lưu đối tượng vào Session (ép kiểu sang chuỗi JSON)
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // 2. Hàm lấy đối tượng từ Session (giải mã chuỗi JSON về lại đối tượng)
        public static T? Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}