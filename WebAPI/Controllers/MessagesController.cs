using AutoMapper;
using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebAPI.DTOs.Message;
using WebAPI.DTOs.User;
using WebAPI.Hubs;
using WebAPI.Other;

namespace WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        private readonly IFileManager _fileManager;

        private readonly IUserOnProjectService _userOnProjectService;

        private readonly IMessageService _messageService;

        private readonly ITaskService _taskService;

        private readonly IMapper _mapper;

        private readonly IHubContext<TasksHub> _tasksHubContext;

        private readonly IHubContext<MessagesHub> _hubContext;

        public MessagesController(UserManager<User> userManager, IUserOnProjectService userOnProjectService, 
            IMessageService messageService, IMapper mapper, ITaskService taskService, IFileManager fileManager, 
            IHubContext<TasksHub> tasksHubContext, IHubContext<MessagesHub> hubContext)
        {
            _userManager = userManager;
            _userOnProjectService = userOnProjectService;
            _messageService = messageService;
            _mapper = mapper;
            _taskService = taskService;
            _fileManager = fileManager;
            _tasksHubContext = tasksHubContext;
            _hubContext = hubContext;
        }

        // GET api/<MessagesController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var model = await _messageService.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            var task = await _taskService.GetByIdAsync(model.TaskId);
            if (task is null || !await _userOnProjectService.IsOnProjectAsync(task.ProjectId, userId.Value))
                return Forbid();
            var result = _mapper.Map<MessageDTO>(model);
            var userModel = await _userManager.FindByIdAsync(model.SenderId.ToString());
            var sender = new UserMiniWithAvatarDTO
            {
                Id = model.SenderId
            };
            if (userModel is not null)
            {
                string? avatarType = null;
                string? avatarURL = null;
                if (userModel.AvatarFormat is not null)
                {
                    avatarType = _fileManager.GetImageMIMEType(userModel.AvatarFormat);
                    avatarURL = $"{this.GetApiUrl()}Users/{userModel.Id}/Avatar";
                }
                sender = sender with
                {
                    FullName = $"{userModel.FirstName} {userModel.LastName}".TrimEnd(),
                    MIMEAvatarType = avatarType,
                    AvatarURL = avatarURL
                };
            }
            result = result with
            {
                Sender = sender
            };
            return Ok(result);
        }

        // PUT api/<MessagesController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AddMessageDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var model = await _messageService.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            if (model.SenderId != userId.Value)
                return Forbid();
            model.Text = dto.Text;
            try
            {
                await _messageService.UpdateAsync(model);

                var connectionIds = Request.Headers["ConnectionId"];
                await _hubContext.Clients.GroupExcept(id.ToString(), connectionIds)
                    .SendAsync("Updated", id, dto);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            return NoContent();
        }

        // DELETE api/<MessagesController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var model = await _messageService.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            if (model.SenderId != userId.Value)
            {
                var task = await _taskService.GetByIdAsync(model.TaskId);
                if (task is null)
                    return Forbid();
                var role = await _userOnProjectService.GetRoleOnProjectAsync(task.ProjectId, userId.Value);
                if (role is null || role < UserOnProjectRoles.Manager)
                    return Forbid();
            }
            try
            {
                await _messageService.DeleteByIdAsync(id);

                var connectionIds = Request.Headers["TasksConnectionId"];
                await _tasksHubContext.Clients.GroupExcept(model.TaskId.ToString(), connectionIds)
                    .SendAsync("DeletedMessage", model.TaskId, id);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
