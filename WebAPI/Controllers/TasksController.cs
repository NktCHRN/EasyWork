﻿using AutoMapper;
using Business.Exceptions;
using Business.Interfaces;
using Business.Models;
using Business.Other;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebAPI.DTOs.File;
using WebAPI.DTOs.Message;
using WebAPI.DTOs.Task;
using WebAPI.DTOs.Task.Executor;
using WebAPI.DTOs.Task.Status;
using WebAPI.DTOs.User;
using WebAPI.Hubs;
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

        private readonly IHubContext<ProjectsHub> _projectsHubContext;

        private readonly IHubContext<TasksHub> _hubContext;

        private readonly IHubContext<UsersHub> _usersHubContext;

        public TasksController(UserManager<User> userManager, IUserOnProjectService userOnProjectService, ITaskService taskService, IMapper mapper, IFileService fileService, IFileManager fileManager, IMessageService messageService, IHubContext<ProjectsHub> projectsHubContext, IHubContext<TasksHub> hubContext, IHubContext<UsersHub> usersHubContext)
        {
            _userManager = userManager;
            _userOnProjectService = userOnProjectService;
            _taskService = taskService;
            _mapper = mapper;
            _fileService = fileService;
            _fileManager = fileManager;
            _messageService = messageService;
            _projectsHubContext = projectsHubContext;
            _hubContext = hubContext;
            _usersHubContext = usersHubContext;
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

        // GET api/<TasksController>/5/reduced
        [HttpGet("{id}/reduced")]
        public async Task<IActionResult> GetReducedById(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var model = await _taskService.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(model.ProjectId, userId.Value))
                return Forbid();
            var result = _mapper.Map<TaskReducedDTO>(model);
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
            var executors = model.ExecutorsIds;
            var oldStatus = model.Status;
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

                var connectionIds = Request.Headers["ConnectionId"];
                await _hubContext.Clients.GroupExcept(id.ToString(), connectionIds).SendAsync("Updated", id, dto);

                if (oldStatus != model.Status)
                {
                    var projectsHubConnectionIds = Request.Headers["ProjectsConnectionId"];
                    await _projectsHubContext.Clients.GroupExcept(model.ProjectId.ToString(), projectsHubConnectionIds)
                        .SendAsync("TaskStatusChanged", model.ProjectId, new StatusChangeDTO
                        {
                            Id = id,
                            Old = oldStatus.ToString(),
                            New = model.Status.ToString()
                        });
                }

                var newIsDone = HelperMethods.IsDoneTask(model.Status);
                if (HelperMethods.IsDoneTask(oldStatus) != newIsDone)
                {
                    var doneValue = IsDoneToShort(newIsDone);
                    var notDoneValue = IsDoneToShort(!newIsDone);
                    foreach (var executor in executors)
                    {
                        await _projectsHubContext.Clients.Group(model.ProjectId.ToString())
                            .SendAsync("TasksDoneChanged", new StatsChangeDTO
                            {
                                ProjectId = model.ProjectId,
                                UserId = executor,
                                Value = doneValue
                            });
                        await _projectsHubContext.Clients.Group(model.ProjectId.ToString())
                            .SendAsync("TasksNotDoneChanged", new StatsChangeDTO
                            {
                                ProjectId = model.ProjectId,
                                UserId = executor,
                                Value = notDoneValue
                            });
                    }
                }
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

        private static short IsDoneToShort(bool isDone) => (short)(isDone ? 1 : -1);

        private static string StatusToMethodName(TaskStatuses status)
            => HelperMethods.IsDoneTask(status) ? "TasksDoneChanged" : "TasksNotDoneChanged";

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
            var executors = model.ExecutorsIds;
            if (!await _userOnProjectService.IsOnProjectAsync(model.ProjectId, userId.Value))
                return Forbid();
            try
            {
                await _taskService.DeleteByIdAsync(id);

                var connectionIds = Request.Headers["ProjectsConnectionId"];
                await _projectsHubContext.Clients.GroupExcept(model.ProjectId.ToString(), connectionIds)
                    .SendAsync("DeletedTask", model.ProjectId, id);
                var methodName = StatusToMethodName(model.Status);
                foreach (var executor in executors)
                {
                    await _projectsHubContext.Clients.Group(model.ProjectId.ToString())
                        .SendAsync(methodName, new StatsChangeDTO
                        {
                            ProjectId = model.ProjectId,
                            UserId = executor,
                            Value = -1
                        });
                }
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

        [Obsolete("Use the chunk file upload instead")]
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
            FileDTO dto;
            try
            {
                await _fileService.AddAsync(fileModel, file);
                dto = new FileDTO()
                {
                    Id = fileModel.Id,
                    Name = fileModel.Name,
                    Size = file.Length,
                    IsFull = fileModel.IsFull
                };

                var connectionIds = Request.Headers["ConnectionId"];
                await _hubContext.Clients.GroupExcept(id.ToString(), connectionIds)
                    .SendAsync("UploadedFile", id, dto);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (InvalidOperationException exc)
            {
                return BadRequest(exc.Message);
            }
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
            FileReducedDTO mapped;
            try
            {
                await _fileService.ChunkAddStartAsync(fileModel);
                mapped = _mapper.Map<FileReducedDTO>(fileModel);

                var connectionIds = Request.Headers["ConnectionId"];
                await _hubContext.Clients.GroupExcept(id.ToString(), connectionIds)
                    .SendAsync("StartedFileUpload", id, mapped);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            return Created($"{this.GetApiUrl()}Files/{fileModel.Id}", mapped);
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
            MessageDTO result;
            try
            {
                await _messageService.AddAsync(message);

                result = _mapper.Map<MessageDTO>(message);
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

                var connectionIds = Request.Headers["ConnectionId"];
                await _hubContext.Clients.GroupExcept(id.ToString(), connectionIds)
                    .SendAsync("AddedMessage", id, result);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
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
            var result = _taskService.GetTaskExecutors(id);
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

                await _usersHubContext.Clients.User(dto.Id.ToString())
                    .SendAsync("AddedAsExecutor", _mapper.Map<UserTaskDTO>(model));

                var connectionIds = Request.Headers["ConnectionId"];
                await _hubContext.Clients.GroupExcept(id.ToString(), connectionIds)
                    .SendAsync("AddedExecutor", id, dto.Id);

                var methodName = StatusToMethodName(model.Status);
                await _projectsHubContext.Clients.Group(model.ProjectId.ToString())
                        .SendAsync(methodName, new StatsChangeDTO
                        {
                            ProjectId = model.ProjectId,
                            UserId = dto.Id,
                            Value = 1
                        });
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

                var connectionIds = Request.Headers["ConnectionId"];
                await _hubContext.Clients.GroupExcept(id.ToString(), connectionIds).SendAsync("DeletedExecutor", id, userId);

                var methodName = StatusToMethodName(model.Status);
                await _projectsHubContext.Clients.Group(model.ProjectId.ToString())
                        .SendAsync(methodName, new StatsChangeDTO
                        {
                            ProjectId = model.ProjectId,
                            UserId = userId,
                            Value = -1
                        });
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
