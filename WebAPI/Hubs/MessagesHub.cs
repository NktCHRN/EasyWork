using Business.Interfaces;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Other;

namespace WebAPI.Hubs
{
    public class MessagesHub : Hub
    {
        private readonly IUserOnProjectService _userOnProjectService;

        private readonly ITaskService _taskService;

        private readonly IMessageService _messageService;

        public MessagesHub(IUserOnProjectService userOnProjectService, ITaskService taskService, IMessageService messageService)
        {
            _userOnProjectService = userOnProjectService;
            _taskService = taskService;
            _messageService = messageService;
        }

        public async Task GetConnectionId()
        {
            await Clients.Caller.SendAsync("ConnectionId", Context.ConnectionId);
        }

        public async Task StartListening(int messageId)
        {
            if (Context.User == null)
                throw new HubException("You are not authorized");
            var userId = Context.User.GetId();
            if (userId == null)
                throw new HubException("You are not authorized");
            var message = await _messageService.GetByIdAsync(messageId);
            if (message == null)
                throw new HubException("The message was not found");
            var task = await _taskService.GetByIdAsync(message.TaskId);
            if (task == null)
                throw new HubException("The task was not found");
            if (!await _userOnProjectService.IsOnProjectAsync(task.ProjectId, userId.Value))
                throw new HubException("You are not a user of this project");
            await Groups.AddToGroupAsync(Context.ConnectionId, messageId.ToString());
        }

        public async Task StopListening(int messageId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, messageId.ToString());
        }
    }
}
