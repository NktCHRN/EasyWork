using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.Other;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserInfoController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        private readonly IFileManager _fileManager;

        public UserInfoController(UserManager<User> manager, IFileManager fileManager)
        {
            _userManager = manager;
            _fileManager = fileManager;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.FindByEmailAsync(User.Identity!.Name);
                var name = user.FirstName;
                if (user.LastName is not null)
                    name += " " + user.LastName;
                string? avatarType = null;
                string? avatarURL = null;
                if (user.AvatarFormat is not null)
                {
                    avatarType = _fileManager.GetImageMIMEType(user.AvatarFormat);
                    avatarURL = $"{this.GetApiUrl()}Users/{user.Id}/Avatar";
                }    
                return Ok(new UserReducedDTO()
                {
                    FullName = name,
                    MIMEAvatarType = avatarType,
                    AvatarURL = avatarURL
                });
        }
    }
}
