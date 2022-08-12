using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using WebAPI.DTOs.UserOnProject;
using WebAPI.Hubs;
using WebAPI.Other;

namespace WebAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InvitesController : ControllerBase
    {
        private readonly IUserOnProjectService _userOnProjectService;

        private readonly IProjectService _projectService;

        private readonly IMapper _mapper;

        private readonly IHubContext<ProjectsHub> _projectsHubContext;

        public InvitesController(IProjectService projectService, IUserOnProjectService userOnProjectService, 
            IMapper mapper, IHubContext<ProjectsHub> projectsHubContext)
        {
            _projectService = projectService;
            _userOnProjectService = userOnProjectService;
            _mapper = mapper;
            _projectsHubContext = projectsHubContext;
        }

        [HttpPost("{inviteCode}")]
        public async Task<IActionResult> ProcessInviteCode(string inviteCode)
        {
            var project = await _projectService.GetProjectByActiveInviteCodeAsync(inviteCode);
            if (project is null)
                return BadRequest("The invite code is not active or invalid");
            var userId = User.GetId();
            if (userId is null)
                return Unauthorized();
            var uop = new UserOnProjectModel()
            {
                ProjectId = project.Id,
                UserId = User.GetId()!.Value,
                Role = UserOnProjectRoles.User
            };
            UserOnProjectDTO mapped;
            try
            {
                await _userOnProjectService.AddAsync(uop);
                mapped = _mapper.Map<UserOnProjectDTO>(uop);
                await _projectsHubContext.Clients.Group(project.Id.ToString()).SendAsync("AddedUser", mapped);
                await _projectsHubContext.Clients.User(userId.Value.ToString()).SendAsync("AddedUser", mapped);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            catch (DbUpdateException exc)
            {
                var message = exc.Message;
                if (exc.InnerException != null)
                {
                    if (exc.InnerException.Message.ToLower().Contains("duplicate"))
                        return Ok(_mapper.Map<UserOnProjectDTO>(uop));
                    message += $"{Environment.NewLine}{exc.InnerException.Message}";
                }
                return BadRequest(message);
            }
            return Created($"{this.GetApiUrl()}Projects/{project.Id}/Users/{uop.UserId}", mapped);
        }
    }
}
