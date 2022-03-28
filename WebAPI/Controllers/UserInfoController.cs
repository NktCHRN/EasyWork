using Business.Interfaces;
using Data.Entities;
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
        private readonly UserManager<User> _Usermanager;

        private readonly IFileManager _fileManager;

        public UserInfoController(UserManager<User> manager, IFileManager fileManager)
        {
            _Usermanager = manager;
            _fileManager = fileManager;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (User.IsAuthenticated())
            {
                var user = await _Usermanager.GetUserAsync(User);
                var name = user.FirstName;
                if (user.LastName is not null)
                    name += " " + user.LastName;
                string? avatarType = null;
                byte[]? avatar = null;
                if (user.AvatarFormat is not null)
                {
                    avatarType = _fileManager.GetImageMIMEType(user.AvatarFormat);
                    avatar = await _fileManager
                        .GetFileContentAsync(user.Id + "." + user.AvatarFormat, Business.Enums.EasyWorkFileTypes.UserAvatar);
                }    
                return Ok(new UserReducedDTO()
                {
                    FullName = name,
                    MIMEAvatarType = avatarType,
                    Avatar = avatar
                });
            }
            return NoContent();
        }
    }
}
