using System.Security.Claims;

namespace WebAPI.Other
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            return !(user.Identity?.IsAuthenticated).GetValueOrDefault()
            ? null
            : int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }

        public static bool IsAuthenticated(this ClaimsPrincipal user)
        {
            return (user.Identity?.IsAuthenticated).GetValueOrDefault();
        }
    }
}
