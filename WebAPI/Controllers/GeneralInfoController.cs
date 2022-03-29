using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GeneralInfoController : ControllerBase
    {
        private readonly IProjectService _projectService;

        private readonly UserManager<User> _manager;

        public GeneralInfoController(IProjectService projectService, UserManager<User> manager)
        {
            _projectService = projectService;
            _manager = manager;
        }

        [HttpGet]
        public GeneralInfoDTO Get()
        {
            return new GeneralInfoDTO()
            {
                ProjectsCount = _projectService.GetCount(),
                UsersCount = _manager.Users.Count()
            };
        }
    }
}
