using AutoMapper;
using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        private readonly IFileManager _fileManager;

        public readonly IUserStatsService _userStatsService;

        private readonly IMapper _mapper;

        public UsersController(IUserStatsService userStatsService, IFileManager fileManager, UserManager<User> userManager, IMapper mapper)
        {
            _userStatsService = userStatsService;
            _fileManager = fileManager;
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var users = _userManager.Users;
            var profiles = new List<UserProfileReducedDTO>();
            foreach (var user in users)
            {
                string? avatarType = null;
                byte[]? avatar = null;
                if (user.AvatarFormat is not null)
                {
                    avatarType = _fileManager.GetImageMIMEType(user.AvatarFormat);
                    avatar = await _fileManager
                        .GetFileContentAsync(user.Id + "." + user.AvatarFormat, Business.Enums.EasyWorkFileTypes.UserAvatar);
                }
                var profile = _mapper.Map<UserProfileReducedDTO>(user);
                profile = profile with
                {
                    MIMEAvatarType = avatarType,
                    Avatar = avatar
                };
                profiles.Add(profile);
            }
            return Ok(profiles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null)
                return NotFound();
            var profile = _mapper.Map<UserProfileDTO>(user);
            var stats = _userStatsService.GetStatsById(user.Id);
            string? avatarType = null;
            byte[]? avatar = null;
            if (user.AvatarFormat is not null)
            {
                avatarType = _fileManager.GetImageMIMEType(user.AvatarFormat);
                avatar = await _fileManager
                    .GetFileContentAsync(user.Id + "." + user.AvatarFormat, Business.Enums.EasyWorkFileTypes.UserAvatar);
            }
            return Ok(profile with
            {
                MIMEAvatarType = avatarType,
                Avatar = avatar,
                Projects = stats.Projects,
                TasksDone = stats.TasksDone,
                TasksNotDone = stats.TasksNotDone
            });
        }
    }
}
