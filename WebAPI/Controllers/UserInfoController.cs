using Business.Interfaces;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using WebAPI.DTOs;
using WebAPI.Other;
using System.Web;
using Microsoft.AspNetCore.StaticFiles;

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
    }
}
