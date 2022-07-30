using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using WebAPI.DTOs.User;
using WebAPI.Interfaces;
using WebAPI.Other;
using Task = System.Threading.Tasks.Task;

namespace WebAPI.Hubs
{
    [Authorize]
    public class ProjectsHub : Hub
    {
        private readonly IUserOnProjectService _userOnProjectService;

        private readonly IProjectUsersContainer _projectUsersContainer;

        private readonly UserManager<User> _userManager;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IFileManager _fileManager;

        public ProjectsHub(IUserOnProjectService userOnProjectService, IProjectUsersContainer projectUsersContainer, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, IFileManager fileManager)
        {
            _userOnProjectService = userOnProjectService;
            _projectUsersContainer = projectUsersContainer;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _fileManager = fileManager;
        }

        [Authorize]
        public async Task Login(int projectId)
        {
            if (Context.User == null)
                throw new HubException("You are not authorized");
            var model = await _userManager.GetUserAsync(Context.User);
            if (model == null)
                throw new HubException("You are not authorized");
            if (_projectUsersContainer.Add(projectId, model.Id, Context.ConnectionId))
                await Clients.Group(projectId.ToString()).SendAsync("Login", projectId, 
                new UserMiniWithAvatarDTO
                {
                    Id = model.Id,
                    FullName = $"{model.FirstName} {model.LastName}".TrimEnd(),
                    AvatarURL = model.AvatarFormat is not null 
                    ? $"{_httpContextAccessor.HttpContext!.Request.GetApiUrl()}Users/{model.Id}/Avatar" 
                    : null,
                    MIMEAvatarType = model.AvatarFormat is not null
                    ? _fileManager.GetImageMIMEType(model.AvatarFormat)
                    : null
            });
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

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.User != null)
            {
                var id = Context.User.GetId();
                if (id != null)
                {
                    var projects = _projectUsersContainer.Delete(id.Value, Context.ConnectionId);
                    foreach (var project in projects)
                        await Clients.Group(project.ToString()).SendAsync("Logout", project, id);
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
