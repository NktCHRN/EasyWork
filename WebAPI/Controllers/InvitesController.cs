using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
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

        public InvitesController(IProjectService projectService, IUserOnProjectService userOnProjectService, IMapper mapper)
        {
            _projectService = projectService;
            _userOnProjectService = userOnProjectService;
            _mapper = mapper;
        }

        [HttpPost("{inviteCode}")]
        public async Task<IActionResult> ProceedInviteCode(string inviteCode)
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
            try
            {
                await _userOnProjectService.AddAsync(uop);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            return Created($"{this.GetApiUrl()}Projects/{project.Id}/Users/{userId}", _mapper.Map<UserOnProjectDTO>(uop));
            // recheck this route!!!
        }
    }
}
