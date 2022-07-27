using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs.Project;
using WebAPI.DTOs.Project.Gantt;
using WebAPI.DTOs.Project.InviteCode;
using WebAPI.DTOs.Project.Limits;
using WebAPI.DTOs.Project.Tasks;
using WebAPI.DTOs.Task;
using WebAPI.DTOs.User;
using WebAPI.DTOs.UserOnProject;
using WebAPI.Hubs;
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

        private readonly ITaskService _taskService;

        private readonly IMapper _mapper;

        private readonly IHubContext<ProjectsHub> _hubContext;

        public ProjectsController(IProjectService projectService, IMapper mapper, IUserOnProjectService userOnProjectService, UserManager<User> userManager, IFileManager fileManager, ITaskService taskService, IHubContext<ProjectsHub> hubContext)
        {
            _projectService = projectService;
            _mapper = mapper;
            _userOnProjectService = userOnProjectService;
            _userManager = userManager;
            _fileManager = fileManager;
            _taskService = taskService;
            _hubContext = hubContext;
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

        [HttpGet("{id}/reduced")]
        public async Task<IActionResult> GetReducedInfoById(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(project.Id, userId.Value))
                return Forbid();
            return Ok(_mapper.Map<ProjectReducedDTO>(project));
        }

        [HttpGet("{id}/limits")]
        public async Task<IActionResult> GetProjectLimits(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var limits = await _projectService.GetLimitsByIdAsync(id);
            if (limits is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(id, userId.Value))
                return Forbid();
            return Ok(_mapper.Map<ProjectLimitsDTO>(limits));
        }

        [HttpPut("{id}/limits")]
        public async Task<IActionResult> UpdateProjectLimits(int id, [FromBody]ProjectLimitsDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var role = await _userOnProjectService.GetRoleOnProjectAsync(id, userId.Value);
            if (role is null || role < UserOnProjectRoles.Manager)
                return Forbid();
            try
            {
                await _projectService.UpdateLimitsByIdAsync(id, _mapper.Map<ProjectLimitsModel>(dto));
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            catch(InvalidOperationException)
            {
                return NotFound();
            }
            return NoContent();
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
                StartDate = DateTimeOffset.UtcNow,
                InviteCode = Guid.NewGuid()
            };
            ProjectDTO mapped;
            try
            {
                await _projectService.AddAsync(project);
                await _userOnProjectService.AddAsync(new UserOnProjectModel()
                {
                    UserId = userId.Value,
                    ProjectId = project.Id,
                    Role = UserOnProjectRoles.Owner
                });
                mapped = _mapper.Map<ProjectDTO>(project);
                await _hubContext.Clients.User(userId.Value.ToString()).SendAsync("Added", mapped);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            return Created($"{this.GetApiUrl()}Projects/{project.Id}", mapped);
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
            if (role is null || role < UserOnProjectRoles.Owner)
                return Forbid();
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound();
            project = _mapper.Map(dto, project);
            try
            {
                await _projectService.UpdateAsync(project);
                var connectionIds = Request.Headers["ConnectionId"];
                await _hubContext.Clients.GroupExcept(id.ToString(), connectionIds).SendAsync("Updated", id, dto);
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
                var connectionIds = Request.Headers["ConnectionId"];
                await _hubContext.Clients.GroupExcept(id.ToString(), connectionIds).SendAsync("Deleted", id);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPut("{id}/inviteStatus")]
        public async Task<IActionResult> UpdateInviteCodeStatus(int id, [FromBody] UpdateInviteCodeStatusDTO dto)
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

        [HttpGet("{id}/invite")]
        public async Task<IActionResult> GetInviteCodeById(int id)
        {
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound();
            if (!await _userOnProjectService.IsOnProjectAsync(project.Id, userId.Value))
                return Forbid();
            return Ok(_mapper.Map<InviteCodeDTO>(project));
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
            return Created($"{this.GetApiUrl()}Invites/{project.InviteCode}", project.InviteCode);
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
                var userDTO = new UserMiniWithAvatarDTO
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
                        FullName = $"{userModel.FirstName} {userModel.LastName}".TrimEnd(),
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
            return Ok(_mapper.Map<UserOnProjectReducedDTO>(uop));
        }

        [HttpGet("{id}/me")]
        public async Task<IActionResult> GetMeAsProjectUser(int id)
        {
            var myId = User.GetId();
            if (myId is null)
                return Unauthorized();
            var project = await _projectService.GetByIdAsync(id);
            if (project is null)
                return NotFound();
            var uop = await _userOnProjectService.GetByIdAsync(id, myId.Value);
            if (uop is null)
                return NotFound();
            return Ok(_mapper.Map<UserOnProjectReducedDTO>(uop));
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
                var connectionIds = Request.Headers["ConnectionId"];
                await _hubContext.Clients.GroupExcept(id.ToString(), connectionIds).SendAsync("DeletedUser", id, userId);
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
                var connectionIds = Request.Headers["ConnectionId"];
                await _hubContext.Clients.GroupExcept(id.ToString(), connectionIds).SendAsync("DeletedUser", id, userId);
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
            UserOnProjectDTO mapped;
            try
            {
                await _userOnProjectService.AddAsync(model);
                mapped = _mapper.Map<UserOnProjectDTO>(model);
                var connectionIds = Request.Headers["ConnectionId"];
                await _hubContext.Clients.GroupExcept(id.ToString(), connectionIds).SendAsync("AddedUser", mapped);
                await _hubContext.Clients.User(dto.UserId.ToString()).SendAsync("AddedUser", mapped);
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
            return Created($"{this.GetApiUrl()}Projects/{project.Id}/Users/{dto.UserId}", mapped);
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
        public async Task<IActionResult> GetTasks(int id)
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
                tasks.Add(_taskService.GetProjectTasksByStatus(id, (TaskStatuses)i));
            foreach (var tasksElement in tasks)
            {
                mappedTasks.Add(new List<TaskReducedDTO>());
                foreach (var task in tasksElement)
                {
                    var mappedTask = _mapper.Map<TaskReducedDTO>(task);
                    mappedTasks.Last().Add(mappedTask);
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
            var tasks = _taskService.GetProjectTasksByStatus(id, TaskStatuses.Archived);
            var mappedTasks = new List<TaskReducedDTO>();
            foreach (var task in tasks)
            {
                var mappedTask = _mapper.Map<TaskReducedDTO>(task);
                mappedTasks.Add(mappedTask);
            }
            return Ok(mappedTasks);
        }

        [HttpGet("{id}/gantt")]
        public async Task<IActionResult> GetGanttChart(int id, [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to)
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
                var endDate = (task.EndDate is not null && task.EndDate <= to) ? task.EndDate.Value : to;
                var deadline = (task.Deadline is not null && task.Deadline <= to) ? task.Deadline.Value : to;
                mappedTasks.Add(new GanttTaskDTO()
                {
                    Id = task.Id,
                    Name = task.Name,
                    GanttStartDate = startDate,
                    GanttDeadline = deadline,
                    GanttEndDate = endDate,
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
