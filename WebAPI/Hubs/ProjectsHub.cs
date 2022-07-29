using Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Other;

namespace WebAPI.Hubs
{
    [Authorize]
    public class ProjectsHub : Hub
    {
        private readonly IUserOnProjectService _userOnProjectService;

        public ProjectsHub(IUserOnProjectService userOnProjectService)
        {
            _userOnProjectService = userOnProjectService;
        }

        public async Task StartListening(int projectId)
        {
            if (Context.User == null)
                throw new HubException("You are not authorized");
            var userId = Context.User.GetId();
            if (userId == null)
                throw new HubException("You are not authorized");
            if (!await _userOnProjectService.IsOnProjectAsync(projectId, userId.Value))
                throw new HubException("You are not a user of this project");
            await Groups.AddToGroupAsync(Context.ConnectionId, projectId.ToString());
        }

        public async Task GetConnectionId()
        {
            await Clients.Caller.SendAsync("ConnectionId", Context.ConnectionId);
        }

        public async Task StopListening(int projectId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, projectId.ToString());
        }

    }
}
