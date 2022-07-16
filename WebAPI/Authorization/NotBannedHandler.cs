using Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using WebAPI.Other;

namespace WebAPI.Authorization
{
    public class NotBannedHandler : AuthorizationHandler<NotBannedRequirement>
    {
        private readonly IBanService _banService;

        public NotBannedHandler(IBanService banService)
        {
            _banService = banService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, NotBannedRequirement requirement)
        {
            var id = context.User.GetId();
            if (id == null)
                return;
            if (!await Task.Run(() => _banService.IsBanned(id.Value)))
                context.Succeed(requirement);
        }
    }
}
