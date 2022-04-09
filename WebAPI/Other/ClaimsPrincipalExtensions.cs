using System.Security.Claims;

namespace WebAPI.Other
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetId(this ClaimsPrincipal user)
        {
            if(int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int parsed))
                return parsed;
            return null;
        }
    }
}
