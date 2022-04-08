using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs;
using WebAPI.Other;

namespace WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        private readonly IFileManager _fileManager;

        private readonly IProjectService _projectService;

        private readonly IUserOnProjectService _userOnProjectService;

        private readonly IReleaseService _releaseService;

        private readonly ITagService _tagService;

        private readonly ITaskService _taskService;

        private readonly IMapper _mapper;
        public ProjectsController(IProjectService projectService, IMapper mapper, IUserOnProjectService userOnProjectService, IReleaseService releaseService, ITagService tagService, UserManager<User> userManager, IFileManager fileManager, ITaskService taskService)
        {
            _projectService = projectService;
            _mapper = mapper;
            _userOnProjectService = userOnProjectService;
            _releaseService = releaseService;
            _tagService = tagService;
            _userManager = userManager;
            _fileManager = fileManager;
            _taskService = taskService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            return Ok(_mapper.Map<IEnumerable<ProjectReducedDTO>>(_projectService.GetUserProjects(userId.Value)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(project.Id, userId.Value))
                return Forbid();
            return Ok(_mapper.Map<ProjectDTO>(project));
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddProjectDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var project = new ProjectModel()
            {
                Name = dto.Name,
                Description = dto.Description,
                StartDate = DateTime.Now,
                InviteCode = Guid.NewGuid()
            };
            try
            {
                await _projectService.AddAsync(project);
                await _userOnProjectService.AddAsync(new UserOnProjectModel()
                {
                    UserId = userId.Value,
                    ProjectId = project.Id,
                    Role = UserOnProjectRoles.Owner
                });
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            return Created($"{this.GetApiUrl()}Projects/{project.Id}", _mapper.Map<ProjectDTO>(project));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProjectDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var role = await _userOnProjectService.GetRoleOnProjectAsync(id, userId.Value);
            if (role is null || role < UserOnProjectRoles.Manager)
                return Forbid();
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound();
            if (role == UserOnProjectRoles.Manager && (project.Name != dto.Name || project.Description != dto.Description))
                return Forbid();
            project = _mapper.Map(dto, project);
            try
            {
                await _projectService.UpdateAsync(project);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound();
            var role = await _userOnProjectService.GetRoleOnProjectAsync(id, userId.Value);
            if (role is null || role < UserOnProjectRoles.Owner)
                return Forbid();
            try
            {
                await _projectService.DeleteByIdAsync(id);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPost("{id}/invite")]
        public async Task<IActionResult> GenerateNewInviteCode(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var role = await _userOnProjectService.GetRoleOnProjectAsync(id, userId.Value);
            if (role is null || role < UserOnProjectRoles.Manager)
                return Forbid();
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return BadRequest();
            project.InviteCode = Guid.NewGuid();
            try
            {
                await _projectService.UpdateAsync(project);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            return NoContent();
        }

        [HttpGet("{id}/releases")]
        public async Task<IActionResult> GetProjectReleases(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            if (!await _userOnProjectService.IsOnProjectAsync(id, userId.Value))
                return Forbid();
            return Ok(_mapper.Map<IEnumerable<ReleaseDTO>>(_releaseService.GetProjectReleases(id)));
        }

        [HttpPost("{id}/releases")]
        public async Task<IActionResult> AddRelease(int id, [FromBody]AddReleaseDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var role = await _userOnProjectService.GetRoleOnProjectAsync(id, userId.Value);
            if (role is null || role < UserOnProjectRoles.Manager)
                return Forbid();
            var release = _mapper.Map<ReleaseModel>(dto);
            release.Date = DateTime.Now;
            release.ProjectId = id;
            try
            {
                await _releaseService.AddAsync(release);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            return Created($"{this.GetApiUrl()}Releases/{release.Id}", _mapper.Map<ReleaseDTO>(release));
        }

        [HttpGet("{id}/tags")]
        public async Task<IActionResult> GetProjectTags(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            if (!await _userOnProjectService.IsOnProjectAsync(id, userId.Value))
                return Forbid();
            return Ok(_mapper.Map<IEnumerable<TagDTO>>(_tagService.GetProjectTags(id)));
        }

        [HttpDelete("{id}/tags/{tagId}")]
        public async Task<IActionResult> DeleteTagFromProject(int id, int tagId)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound();
            var role = await _userOnProjectService.GetRoleOnProjectAsync(id, userId.Value);
            if (role is null || role < UserOnProjectRoles.Manager)
                return Forbid();
            try
            {
                await _tagService.DeleteFromProjectByIdAsync(tagId, id);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet("{id}/users")]
        public async Task<IActionResult> GetProjectUsers(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(id, userId.Value))
                return Forbid();
            var users = _userOnProjectService.GetProjectUsers(id);
            var usersMapped = new List<UserOnProjectExtendedDTO>();
            foreach (var user in users)
            {
                var userModel = await _userManager.FindByIdAsync(user.UserId.ToString());
                var userDTO = new UserMiniWithAvatarDTO()
                {
                    Id = user.UserId
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
                    userDTO = userDTO with
                    {
                        FullName = (userModel.LastName is null) ? userModel.FirstName : userModel.FirstName + " " + userModel.LastName,
                        MIMEAvatarType = avatarType,
                        AvatarURL = avatarURL
                    };
                }
                usersMapped.Add(new UserOnProjectExtendedDTO()
                {
                    TasksDone = user.TasksDone,
                    TasksNotDone = user.TasksNotDone,
                    Role = user.Role.ToString(),
                    User = userDTO
                });
            }
            return Ok(usersMapped);
        }

        [HttpGet("{id}/users/{userId}")]
        public async Task<IActionResult> GetProjectUser(int id, int userId)
        {
            var myId = User.GetId();
            if (myId is null)
                return Unauthorized();
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(id, myId.Value))
                return Forbid();
            var uop = await _userOnProjectService.GetByIdAsync(id, userId);
            if (uop is null)
                return NotFound();
            return Ok(_mapper.Map<UserOnProjectDTO>(uop));
        }

        [HttpDelete("{id}/users/{userId}")]
        public async Task<IActionResult> DeleteProjectUser(int id, int userId)
        {
            var myId = User.GetId();
            if (myId is null)
                return Unauthorized();
            if (userId == myId)
                return BadRequest("You cannot delete yourself from the project. Use leave instead");
            var role = await _userOnProjectService.GetRoleOnProjectAsync(id, myId.Value);
            var toDeleteRole = await _userOnProjectService.GetRoleOnProjectAsync(id, userId);
            if (toDeleteRole is null)
                return NotFound();
            if (role is null || role < UserOnProjectRoles.Manager 
                || (role == UserOnProjectRoles.Manager && toDeleteRole >= UserOnProjectRoles.Manager))
                return Forbid();
            try
            {
                await _userOnProjectService.DeleteByIdAsync(id, userId);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}/leave")]
        public async Task<IActionResult> LeaveProject(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            if (!await _userOnProjectService.IsOnProjectAsync(id, userId.Value))
                return NotFound();
            try
            {
                await _userOnProjectService.DeleteByIdAsync(id, userId.Value);
            }
            catch (InvalidOperationException exc)
            {
                return BadRequest(exc.Message);
            }
            return NoContent();
        }

        [HttpPost("{id}/users")]
        public async Task<IActionResult> AddUser(int id, [FromBody] AddUserOnProjectDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var myId = User.GetId();
            if (myId is null)
                return Unauthorized();
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound();
            var myRole = await _userOnProjectService.GetRoleOnProjectAsync(id, myId.Value);
            var isValidRole = Enum.TryParse(dto.Role, out UserOnProjectRoles toAddRole);
            if (!isValidRole)
                return BadRequest("Invalid role");
            if (myRole is null || myRole < UserOnProjectRoles.Manager
                || (myRole == UserOnProjectRoles.Manager && toAddRole >= UserOnProjectRoles.Manager))
                return Forbid();
            var model = new UserOnProjectModel()
            {
                ProjectId = id,
                UserId = dto.UserId,
                Role = toAddRole
            };
            try
            {
                await _userOnProjectService.AddAsync(model);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (DbUpdateException)
            {
                return BadRequest("This user is already on the project");
            }
            catch (InvalidOperationException)
            {
                return BadRequest("This user is already on the project");
            }
            return Created($"{this.GetApiUrl()}Projects/{project.Id}/Users/{dto.UserId}", _mapper.Map<UserOnProjectDTO>(model));
        }

        [HttpPut("{id}/users/{userId}")]
        public async Task<IActionResult> UpdateUser(int id, int userId, UpdateUserOnProjectDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var myId = User.GetId();
            if (myId is null)
                return Unauthorized();
            var myRole = await _userOnProjectService.GetRoleOnProjectAsync(id, myId.Value);
            var isValidRole = Enum.TryParse(dto.Role, out UserOnProjectRoles toUpdateRole);
            if (!isValidRole)
                return BadRequest("Invalid role");
            var model = await _userOnProjectService.GetByIdAsync(id, userId);
            if (model is null)
                return NotFound();
            if (myRole is null || myRole < UserOnProjectRoles.Owner)
                return Forbid();
            model.Role = toUpdateRole;
            try
            {
                await _userOnProjectService.UpdateAsync(model);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (InvalidOperationException exc)
            {
                return BadRequest(exc.Message);
            }
            return NoContent();
        }

        [HttpGet("{id}/tasks")]
        public async Task<IActionResult> GetTasks(int id, [FromQuery] int? tagId)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(id, userId.Value))
                return Forbid();
            var tasks = new List<IEnumerable<TaskModel>>();
            var mappedTasks = new List<List<TaskReducedDTO>>();
            for (short i = 0; i < 4; i++)
                tasks.Add(_taskService.GetProjectTasksByStatusAndTag(id, (TaskStatuses)i, tagId));
            foreach (var tasksElement in tasks)
            {
                mappedTasks.Add(new List<TaskReducedDTO>());
                foreach (var task in tasksElement)
                {
                    var mappedTask = _mapper.Map<TaskReducedDTO>(task);
                    UserMiniWithAvatarDTO? executor = null;
                    if (task.ExecutorId is not null)
                    {
                        var userModel = await _userManager.FindByIdAsync(task.ExecutorId.ToString());
                        executor = new UserMiniWithAvatarDTO()
                        {
                            Id = task.ExecutorId.Value
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
                    }
                    mappedTasks.Last().Add(mappedTask with
                    {
                        Executor = executor
                    });
                }
            }
            var result = new TasksDTO()
            {
                ToDo = mappedTasks.ElementAt(0),
                InProgress = mappedTasks.ElementAt(1),
                Validate = mappedTasks.ElementAt(2),
                Done = mappedTasks.ElementAt(3)
            };
            return Ok(result);
        }

        [HttpGet("{id}/archive")]
        public async Task<IActionResult> GetArchivedTasks(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(id, userId.Value))
                return Forbid();
            var tasks = _taskService.GetProjectTasksByStatusAndTag(id, TaskStatuses.Archived);
            var mappedTasks = new List<TaskReducedDTO>();
            foreach (var task in tasks)
            {
                var mappedTask = _mapper.Map<TaskReducedDTO>(task);
                UserMiniWithAvatarDTO? executor = null;
                if (task.ExecutorId is not null)
                {
                    var userModel = await _userManager.FindByIdAsync(task.ExecutorId.ToString());
                    executor = new UserMiniWithAvatarDTO()
                    {
                        Id = task.ExecutorId.Value
                    };
                    if (userModel is not null)
                    {
                        string? avatarType = null;
                        string? avatarURL = null;
                        if (userModel.AvatarFormat is not null)
                        {
                            avatarType = _fileManager.GetImageMIMEType(userModel.AvatarFormat);
                            avatarURL = $"{this.GetApiUrl()}Users/{userModel.Id}/Avatar"; ;
                        }
                        executor = executor with
                        {
                            FullName = (userModel.LastName is null) ? userModel.FirstName : userModel.FirstName + " " + userModel.LastName,
                            MIMEAvatarType = avatarType,
                            AvatarURL = avatarURL
                        };
                    }
                }
                mappedTasks.Add(mappedTask with
                {
                    Executor = executor
                });
            }
            return Ok(mappedTasks);
        }

        [HttpGet("{id}/gantt")]
        public async Task<IActionResult> GetGanttChart(int id, [FromQuery]DateTime from, [FromQuery]DateTime to)
        {
            if (from >= to)
                return BadRequest("\"From\" should be earlier than \"to\"");
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(id, userId.Value))
                return Forbid();
            var tasks = _taskService.GetProjectTasksByDate(id, from, to);
            var mappedTasks = new List<GanttTaskDTO>();
            foreach (var task in tasks)
            {
                var startDate = (task.StartDate >= from) ? task.StartDate : from;
                var deadline = (task.Deadline is not null && task.Deadline <= to) ? task.Deadline.Value : to;
                var endDate = (task.EndDate is not null && task.EndDate <= to) ? task.EndDate.Value : to;
                UserMiniReducedDTO? executor = null;
                if (task.ExecutorId is not null)
                {
                    var userModel = await _userManager.FindByIdAsync(task.ExecutorId.ToString());
                    executor = new UserMiniReducedDTO()
                    {
                        Id = task.ExecutorId.Value
                    };
                    if (userModel is not null)
                    {
                        executor = executor with
                        {
                            FullName = (userModel.LastName is null) ? userModel.FirstName : userModel.FirstName + " " + userModel.LastName
                        };
                    }
                }
                mappedTasks.Add(new GanttTaskDTO()
                {
                    Id = task.Id,
                    Name = task.Name,
                    GanttStartDate = startDate,
                    GanttDeadline = deadline,
                    GanttEndDate = endDate,
                    Executor = executor
                });
            }
            var result = new GanttDTO()
            {
                StartDate = from,
                EndDate = to,
                Tasks = mappedTasks
            };
            return Ok(result);
        }

        [HttpPost("{id}/tasks")]
        public async Task<IActionResult> AddTask(int id, [FromBody] AddTaskDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(id, userId.Value))
                return Forbid();
            var isValidStatus = Enum.TryParse(dto.Status, out TaskStatuses status);
            if (!isValidStatus || status == TaskStatuses.Archived)
                return BadRequest("Invalid or not suitable task status");
            var model = new TaskModel()
            {
                Name = dto.Name,
                Status = status,
                ProjectId = id
            };
            try
            {
                await _taskService.AddAsync(model);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (InvalidOperationException exc)
            {
                return BadRequest(exc.Message);
            }
            return Created($"{this.GetApiUrl()}Tasks/{model.Id}", _mapper.Map<TaskDTO>(model));
        }
    }
}
