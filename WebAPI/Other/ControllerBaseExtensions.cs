using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Other
{
    public static class ControllerBaseExtensions
    {
        public static string GetApiUrl(this ControllerBase controller)
        {
            return $"{controller.Request.Scheme}://{controller.Request.Host.Value}/api/";
        }
    }
}
