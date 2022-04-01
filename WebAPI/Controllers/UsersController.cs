﻿using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
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

        public readonly IBanService _banService;

        private readonly IMapper _mapper;

        public UsersController(IUserStatsService userStatsService, IFileManager fileManager, UserManager<User> userManager, IMapper mapper, IBanService banService)
        {
            _userStatsService = userStatsService;
            _fileManager = fileManager;
            _userManager = userManager;
            _mapper = mapper;
            _banService = banService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var users = _userManager.Users.Where(u => u.EmailConfirmed);
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
            if (user is null || !user.EmailConfirmed)
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

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}/bans")]
        public async Task<IActionResult> GetUserBans(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null || !user.EmailConfirmed)
                return NotFound();
            var bans = _banService.GetUserBans(user.Id);
            return Ok(await MapBansAsync(bans));
        }

        internal async Task<IEnumerable<BannedUserDTO>> MapBansAsync(IEnumerable<BanModel> bans)
        {
            var mappedBans = new List<BannedUserDTO>();
            foreach (var ban in bans)
            {
                var mappedBan = _mapper.Map<BannedUserDTO>(ban);
                var admin = await _userManager.FindByIdAsync(ban.AdminId.GetValueOrDefault().ToString());
                if (admin is not null)
                {
                    var adminModel = new UserMiniDTO()
                    {
                        Id = admin.Id,
                        Email = admin.Email,
                        FullName = (admin.LastName is null) ? admin.FirstName : admin.FirstName + " " + admin.LastName,
                    };
                    mappedBan = mappedBan with
                    {
                        Admin = adminModel
                    };
                }
                mappedBans.Add(mappedBan);
            }
            return mappedBans;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}/activeBans")]
        public async Task<IActionResult> GetActiveUserBans(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null || !user.EmailConfirmed)
                return NotFound();
            var bans = _banService.GetActiveUserBans(user.Id);
            return Ok(await MapBansAsync(bans));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}/unban")]
        public async Task<IActionResult> UnbanUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user is null || !user.EmailConfirmed)
                return NotFound();
            if (!_banService.IsBanned(id))
                return NotFound("This user is not banned");
            await _banService.DeleteActiveUserBansAsync(user.Id);
            return NoContent();
        }
    }
}