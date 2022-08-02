using Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using WebAPI.Other;

namespace WebAPI.Hubs
{
    [Authorize]
    public class FilesHub : Hub
    {
        private readonly IUserOnProjectService _userOnProjectService;

        private readonly ITaskService _taskService;

        private readonly IFileService _fileService;

        public FilesHub(IUserOnProjectService userOnProjectService, ITaskService taskService, IFileService fileService)
        {
            _userOnProjectService = userOnProjectService;
            _taskService = taskService;
            _fileService = fileService;
        }

        public async Task GetConnectionId()
        {
            await Clients.Caller.SendAsync("ConnectionId", Context.ConnectionId);
        }

        public async Task StartListening(int fileId)
        {
            if (Context.User == null)
                throw new HubException("You are not authorized");
            var userId = Context.User.GetId();
            if (userId == null)
                throw new HubException("You are not authorized");
            var fileModel = await _fileService.GetByIdAsync(fileId);
            if (fileModel == null)
                throw new HubException("The file was not found");
            var task = await _taskService.GetByIdAsync(fileModel.TaskId);
            if (task == null)
                throw new HubException("The task was not found");
            if (!await _userOnProjectService.IsOnProjectAsync(task.ProjectId, userId.Value))
                throw new HubException("You are not a user of this project");
            await Groups.AddToGroupAsync(Context.ConnectionId, fileId.ToString());
        }

        public async Task StopListening(int fileId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, fileId.ToString());
        }
    }
}
