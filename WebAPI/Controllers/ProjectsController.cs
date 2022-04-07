using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.Other;

namespace WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        private readonly IUserOnProjectService _userOnProjectService;

        private readonly IReleaseService _releaseService;

        private readonly ITagService _tagService;

        private readonly IMapper _mapper;
        public ProjectsController(IProjectService projectService, IMapper mapper, IUserOnProjectService userOnProjectService, IReleaseService releaseService, ITagService tagService)
        {
            _projectService = projectService;
            _mapper = mapper;
            _userOnProjectService = userOnProjectService;
            _releaseService = releaseService;
            _tagService = tagService;
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
    }
}
