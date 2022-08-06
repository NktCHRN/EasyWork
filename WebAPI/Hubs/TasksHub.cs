using Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Other;

namespace WebAPI.Hubs
{
    [Authorize]
    public class TasksHub : Hub
    {
        private readonly IUserOnProjectService _userOnProjectService;

        private readonly ITaskService _taskService;

        public TasksHub(IUserOnProjectService userOnProjectService, ITaskService taskService)
        {
            _userOnProjectService = userOnProjectService;
            _taskService = taskService;
        }

        public async Task GetConnectionId()
        {
            await Clients.Caller.SendAsync("ConnectionId", Context.ConnectionId);
        }

        public async Task StartListening(int taskId)
        {
            if (Context.User == null)
                throw new HubException("You are not authorized");
            var userId = Context.User.GetId();
            if (userId == null)
                throw new HubException("You are not authorized");
            var task = await _taskService.GetByIdAsync(taskId);
            if (task == null)
                throw new HubException("The task was not found");
            if (!await _userOnProjectService.IsOnProjectAsync(task.ProjectId, userId.Value))
                throw new HubException("You are not a user of this project");
            await Groups.AddToGroupAsync(Context.ConnectionId, taskId.ToString());
        }

        public async Task StopListening(int taskId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, taskId.ToString());
        }
    }
}
