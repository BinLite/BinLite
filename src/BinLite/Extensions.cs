using System.Security.Claims;

namespace BinLite
{
    public static class Extensions
    {
        public static ulong ID(this ClaimsPrincipal user) =>
            ulong.Parse(user.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier").Value);

        public static string Username(this ClaimsPrincipal user) =>
            user.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name").Value;

        public static string Discriminator(this ClaimsPrincipal user) =>
            user.Claims.FirstOrDefault(c => c.Type == "urn:discord:user:discriminator").Value;

        public static string Avatar(this ClaimsPrincipal user) =>
            user.Claims.FirstOrDefault(c => c.Type == "urn:discord:avatar:hash").Value;

        public static string AvatarUrl(this ClaimsPrincipal user, int size) =>
            "https://cdn.discordapp.com/avatars/" + user.ID() + "/" + user.Avatar() + ".png?size=" + size;

    }
}
