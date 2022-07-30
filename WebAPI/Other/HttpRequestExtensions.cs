namespace WebAPI.Other
{
    public static class HttpRequestExtensions
    {
        public static string GetApiUrl(this HttpRequest request)
        {
            return $"{request.Scheme}://{request.Host.Value}/api/";
        }
    }
}
