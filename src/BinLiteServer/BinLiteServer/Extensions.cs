using Microsoft.AspNetCore.Http;

using Newtonsoft.Json;

namespace BinLiteServer
{
    public static class Extensions
    {
        public static string ToJson(this object t) => JsonConvert.SerializeObject(t);
        public static T FromJson<T>(this string t) => JsonConvert.DeserializeObject<T>(t)!;
        public static string Caller(this HttpContext ctx) => ctx.User.Claims.FirstOrDefault(c => c.Type == "id")?.Value!;
    }
}
