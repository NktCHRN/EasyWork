using AutoMapper;
using Business.Exceptions;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs.File;
using WebAPI.DTOs.Message;
using WebAPI.DTOs.Task;
using WebAPI.DTOs.Task.Executor;
using WebAPI.DTOs.User;
using WebAPI.Other;

namespace WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        private readonly IUserOnProjectService _userOnProjectService;

        private readonly ITaskService _taskService;

        private readonly IFileService _fileService;

        private readonly IFileManager _fileManager;

        private readonly IMessageService _messageService;

        private readonly IMapper _mapper;

        public TasksController(UserManager<User> userManager, IUserOnProjectService userOnProjectService, ITaskService taskService, IMapper mapper, IFileService fileService, IFileManager fileManager, IMessageService messageService)
        {
            _userManager = userManager;
            _userOnProjectService = userOnProjectService;
            _taskService = taskService;
            _mapper = mapper;
            _fileService = fileService;
            _fileManager = fileManager;
            _messageService = messageService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var tasks = _taskService.GetUserTasks(userId.Value);
            return Ok(_mapper.Map<IEnumerable<UserTaskDTO>>(tasks));
        }

        // GET api/<TasksController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var model = await _taskService.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(model.ProjectId, userId.Value))
                return Forbid();
            var result = _mapper.Map<TaskDTO>(model);
            return Ok(result);
        }

        // PUT api/<TasksController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var model = await _taskService.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(model.ProjectId, userId.Value))
                return Forbid();
            var isValidStatus = Enum.TryParse(dto.Status, out TaskStatuses status);
            if (!isValidStatus)
                return BadRequest("Invalid task status");
            TaskPriorities? priority = null;
            if (dto.Priority is not null)
            {
                var isValidPriority = Enum.TryParse(dto.Priority, out TaskPriorities temp);
                if (!isValidPriority)
                    return BadRequest("Invalid task priority");
                priority = temp;
            }
            model = _mapper.Map(dto, model);
            model.Status = status;
            model.Priority = priority;
            try
            {
                await _taskService.UpdateAsync(model);
            }
            catch (LimitsExceededException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            return NoContent();
        }

        // DELETE api/<TasksController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var model = await _taskService.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(model.ProjectId, userId.Value))
                return Forbid();
            try
            {
                await _taskService.DeleteByIdAsync(id);
            }
            catch (InvalidOperationException exc)
            {
                return BadRequest(exc.Message);
            }
            return NoContent();
        }

        [HttpGet("{id}/files")]
        public async Task<IActionResult> GetTaskFileModels(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var model = await _taskService.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(model.ProjectId, userId.Value))
                return Forbid();
            var files = await _fileService.GetTaskFilesAsync(id);
            return Ok(_mapper.Map<IEnumerable<FileDTO>>(files));
        }

        [HttpPost("{id}/files")]
        public async Task<IActionResult> AddFile(int id, IFormFile file)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var model = await _taskService.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(model.ProjectId, userId.Value))
                return Forbid();
            var fileModel = new FileModel
            {
                Name = file.FileName,
                TaskId = id
            };
            try
            {
                await _fileService.AddAsync(fileModel, file);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (InvalidOperationException exc)
            {
                return BadRequest(exc.Message);
            }
            var dto = new FileDTO()
            {
                Id = fileModel.Id,
                Name = fileModel.Name,
                Size = file.Length,
                IsFull = fileModel.IsFull
            };
            return Created($"{this.GetApiUrl()}Files/{fileModel.Id}", dto);
        }

        [HttpPost("{id}/files/start")]
        public async Task<IActionResult> StartChunkFileAdd(int id, AddFileDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var model = await _taskService.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(model.ProjectId, userId.Value))
                return Forbid();
            var fileModel = new FileModel
            {
                Name = dto.Name,
                TaskId = id
            };
            try
            {
                await _fileService.ChunkAddStartAsync(fileModel);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            return Created($"{this.GetApiUrl()}Files/{fileModel.Id}", _mapper.Map<FileReducedDTO>(fileModel));
        }

        [HttpGet("{id}/messages")]
        public async Task<IActionResult> GetTaskMessages(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var model = await _taskService.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(model.ProjectId, userId.Value))
                return Forbid();
            var messages = _messageService.GetTaskMessages(id);
            var mappedMessages = new List<MessageDTO>();
            foreach (var message in messages)
            {
                var mappedMessage = _mapper.Map<MessageDTO>(message);
                var userModel = await _userManager.FindByIdAsync(message.SenderId.ToString());
                var sender = new UserMiniWithAvatarDTO()
                {
                    Id = message.SenderId
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
                mappedMessages.Add(mappedMessage with
                {
                    Sender = sender
                });
            }
            return Ok(mappedMessages);
        }

        [HttpPost("{id}/messages")]
        public async Task<IActionResult> AddMessage(int id, [FromBody] AddMessageDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var model = await _taskService.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(model.ProjectId, userId.Value))
                return Forbid();
            var message = new MessageModel()
            {
                Text = dto.Text,
                SenderId = userId.Value,
                Date = DateTimeOffset.UtcNow,
                TaskId = id
            };
            try
            {
                await _messageService.AddAsync(message);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            var result = _mapper.Map<MessageDTO>(message);
            var userModel = await _userManager.FindByIdAsync(message.SenderId.ToString());
            var sender = new UserMiniWithAvatarDTO()
            {
                Id = message.SenderId
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
            return Created($"{this.GetApiUrl()}Messages/{message.Id}", result);
        }

        [HttpGet("{id}/users")]
        public async Task<IActionResult> GetTaskExecutors(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var model = await _taskService.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(model.ProjectId, userId.Value))
                return Forbid();
            var result = await _taskService.GetTaskExecutorsAsync(id);
            var mapped = new List<UserMiniWithAvatarDTO>();
            foreach (var user in result)
            {
                string? avatarType = null;
                string? avatarURL = null;
                if (user.AvatarFormat is not null)
                {
                    avatarType = _fileManager.GetImageMIMEType(user.AvatarFormat);
                    avatarURL = $"{this.GetApiUrl()}Users/{user.Id}/Avatar";
                }
                mapped.Add(new UserMiniWithAvatarDTO()
                {
                    Id = user.Id,
                    FullName = $"{user.FirstName} {user.LastName}".TrimEnd(),
                    MIMEAvatarType = avatarType,
                    AvatarURL = avatarURL
                });
            }
            return Ok(mapped);
        }

        [HttpPost("{id}/users")]
        public async Task<IActionResult> AddUser(int id, [FromBody] AddExecutorDTO dto)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var model = await _taskService.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(model.ProjectId, userId.Value))
                return Forbid();
            try
            {
                await _taskService.AddExecutorToTaskAsync(id, dto.Id);
            }
            catch (InvalidOperationException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            string? avatarType = null;
            string? avatarURL = null;
            var user = await _userManager.FindByIdAsync(dto.Id.ToString());
            UserMiniWithAvatarDTO createdResult = new()
            {
                Id = dto.Id
            };
            if (user != null)
            {
                if (user.AvatarFormat is not null)
                {
                    avatarType = _fileManager.GetImageMIMEType(user.AvatarFormat);
                    avatarURL = $"{this.GetApiUrl()}Users/{user.Id}/Avatar";
                }
                createdResult = createdResult with
                {
                    FullName = $"{user.FirstName} {user.LastName}".TrimEnd(),
                    MIMEAvatarType = avatarType,
                    AvatarURL = avatarURL
                };
            }
            return Created($"{this.GetApiUrl()}Tasks/{id}/Users", createdResult);
        }

        [HttpDelete("{id}/users/{userId}")]
        public async Task<IActionResult> DeleteUser(int id, int userId)
        {
            var myId = User.GetId();
            if (myId is null)
                return Unauthorized();
            var model = await _taskService.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(model.ProjectId, myId.Value))
                return Forbid();
            try
            {
                await _taskService.DeleteExecutorFromTaskAsync(id, userId);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
