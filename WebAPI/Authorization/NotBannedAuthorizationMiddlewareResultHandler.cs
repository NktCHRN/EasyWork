using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http.Extensions;

namespace WebAPI.Authorization
{
    public class NotBannedAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler DefaultHandler = new();

        public async Task HandleAsync(
            RequestDelegate requestDelegate,
            HttpContext httpContext,
            AuthorizationPolicy authorizationPolicy,
            PolicyAuthorizationResult policyAuthorizationResult)
        {

            if (policyAuthorizationResult.Forbidden &&
                policyAuthorizationResult.AuthorizationFailure != null &&
                policyAuthorizationResult.AuthorizationFailure.FailedRequirements.OfType<NotBannedRequirement>().Any())
            {
                httpContext.Response.StatusCode = 403;
                httpContext.Response.ContentType = "application/json";
                await httpContext.Response.WriteAsJsonAsync("You are banned");
                return;
            }

            await DefaultHandler.HandleAsync(requestDelegate, httpContext, authorizationPolicy,
                                   policyAuthorizationResult);
        }
    }
}
