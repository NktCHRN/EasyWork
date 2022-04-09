using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
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

        private readonly ITagService _tagService;

        private readonly ITaskService _taskService;

        private readonly IFileService _fileService;

        private readonly IFileManager _fileManager;

        private readonly IMessageService _messageService;

        private readonly IMapper _mapper;

        public TasksController(UserManager<User> userManager, IUserOnProjectService userOnProjectService, ITagService tagService, ITaskService taskService, IMapper mapper, IFileService fileService, IFileManager fileManager, IMessageService messageService)
        {
            _userManager = userManager;
            _userOnProjectService = userOnProjectService;
            _tagService = tagService;
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
            if (model.ExecutorId is not null)
            {
                var userModel = await _userManager.FindByIdAsync(model.ExecutorId.ToString());
                var executor = new UserMiniWithAvatarDTO()
                {
                    Id = model.ExecutorId.Value
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
                    executor = executor with
                    {
                        FullName = (userModel.LastName is null) ? userModel.FirstName : userModel.FirstName + " " + userModel.LastName,
                        MIMEAvatarType = avatarType,
                        AvatarURL = avatarURL
                    };
                }
                result = result with
                {
                    Executor = executor
                };
            }
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

        [HttpGet("{id}/tags")]
        public async Task<IActionResult> GetTaskTags(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var model = await _taskService.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(model.ProjectId, userId.Value))
                return Forbid();
            var result = await _tagService.GetTaskTagsAsync(id);
            return Ok(_mapper.Map<IEnumerable<TagDTO>>(result));
        }

        [HttpPost("{id}/tags")]
        public async Task<IActionResult> AddTag(int id, [FromBody] AddTagDTO dto)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var model = await _taskService.GetByIdAsync(id);
            if (model is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(model.ProjectId, userId.Value))
                return Forbid();
            var foundTag = await _tagService.FindByName(dto.Name);
            int tagId;
            if (foundTag is null)
            {
                try
                {
                    var tag = new Business.Models.TagModel
                    {
                        Name = dto.Name
                    };
                    await _tagService.AddAsync(tag);
                    tagId = tag.Id;
                }
                catch (ArgumentException exc)
                {
                    return BadRequest(exc.Message);
                }
            }
            else
                tagId = foundTag.Id;
            try
            {
                await _taskService.AddTagToTaskAsync(id, tagId);
            }
            catch (InvalidOperationException exc)
            {
                return BadRequest(exc.Message);
            }
            return Created($"{this.GetApiUrl()}Tasks/{id}/Tags", new TagDTO()
            {
                Id = tagId,
                Name = dto.Name
            });
        }

        [HttpDelete("{id}/tags/{tagId}")]
        public async Task<IActionResult> DeleteTag(int id, int tagId)
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
                await _taskService.DeleteTagFromTaskAsync(id, tagId);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
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
            return Ok(_mapper.Map<IEnumerable<FileModelDTO>>(files));
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
            var dto = new FileModelDTO()
            {
                Id = fileModel.Id,
                Name = fileModel.Name,
                Size = file.Length
            };
            return Created($"{this.GetApiUrl()}Files/{fileModel.Id}", dto);
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
                        FullName = (userModel.LastName is null) ? userModel.FirstName : userModel.FirstName + " " + userModel.LastName,
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
                Date = DateTime.Now,
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
                    FullName = (userModel.LastName is null) ? userModel.FirstName : userModel.FirstName + " " + userModel.LastName,
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
    }
}
