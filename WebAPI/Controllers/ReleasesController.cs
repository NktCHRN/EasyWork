using AutoMapper;
using Business.Interfaces;
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
    public class ReleasesController : ControllerBase
    {
        private readonly IProjectService _projectService;

        private readonly IUserOnProjectService _userOnProjectService;

        private readonly IReleaseService _releaseService;

        private readonly IMapper _mapper;
        public ReleasesController(IProjectService projectService, IMapper mapper, IUserOnProjectService userOnProjectService, IReleaseService releaseService)
        {
            _projectService = projectService;
            _mapper = mapper;
            _userOnProjectService = userOnProjectService;
            _releaseService = releaseService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var release = await _releaseService.GetByIdAsync(id);
            if (release is null)
                return NotFound();
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            if (!await _userOnProjectService.IsOnProjectAsync(release.ProjectId, userId.Value))
                return Forbid();
            return Ok(_mapper.Map<ReleaseDTO>(release));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AddReleaseDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var release = await _releaseService.GetByIdAsync(id);
            if (release is null)
                return NotFound();
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var role = await _userOnProjectService.GetRoleOnProjectAsync(release.ProjectId, userId.Value);
            if (role is null || role < UserOnProjectRoles.Manager)
                return Forbid();
            release = _mapper.Map(dto, release);
            try
            {
                await _releaseService.UpdateAsync(release);
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
            var release = await _releaseService.GetByIdAsync(id);
            if (release is null)
                return NotFound();
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var role = await _userOnProjectService.GetRoleOnProjectAsync(release.ProjectId, userId.Value);
            if (role is null || role < UserOnProjectRoles.Manager)
                return Forbid();
            try
            {
                await _releaseService.DeleteByIdAsync(id);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
