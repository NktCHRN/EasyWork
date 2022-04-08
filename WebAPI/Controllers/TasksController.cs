using AutoMapper;
using Business.Interfaces;
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

        private readonly IUserAvatarService _userAvatarService;

        private readonly IMapper _mapper;

        public TasksController(UserManager<User> userManager, IUserOnProjectService userOnProjectService, ITagService tagService, ITaskService taskService, IMapper mapper, IUserAvatarService userAvatarService)
        {
            _userManager = userManager;
            _userOnProjectService = userOnProjectService;
            _tagService = tagService;
            _taskService = taskService;
            _mapper = mapper;
            _userAvatarService = userAvatarService;
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
                result = result with
                {
                    Executor = _mapper.Map<UserMiniWithAvatarDTO?>(
                        await _userAvatarService.GetDossierByIdAsync(model.ExecutorId.Value))
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
            return StatusCode(201);
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
    }
}
