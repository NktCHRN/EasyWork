using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebAPI.DTOs.Ban;
using WebAPI.DTOs.User;
using WebAPI.Hubs;
using WebAPI.Other;

namespace WebAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class BansController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        private readonly IBanService _banService;

        private readonly IMapper _mapper;

        private readonly IRefreshTokenService _refreshTokenService;

        private readonly IHubContext<UsersHub> _usersHubContext;

        private readonly IHubContext<UserBansHub> _hubContext;

        public BansController(IMapper mapper, UserManager<User> userManager, IBanService banService,
            IRefreshTokenService refreshTokenService, IHubContext<UsersHub> usersHubContext, 
            IHubContext<UserBansHub> hubContext)
        {
            _mapper = mapper;
            _userManager = userManager;
            _banService = banService;
            _refreshTokenService = refreshTokenService;
            _usersHubContext = usersHubContext;
            _hubContext = hubContext;
        }

        private async Task<UserMiniDTO?> GetUserMiniDTOAsync(int? userId)
        {
            var user = await _userManager.FindByIdAsync(userId.GetValueOrDefault().ToString());
            UserMiniDTO? userModel = null;
            if (user is not null)
            {
                userModel = new UserMiniDTO()
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = $"{user.FirstName} {user.LastName}".TrimEnd(),
                };
            }
            return userModel;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var found = await _banService.GetByIdAsync(id);
            if (found is null)
                return NotFound();
            var mapped = _mapper.Map<BanDTO>(found);
            mapped = mapped with
            {
                User = (await GetUserMiniDTOAsync(found.UserId))!,
                Admin = await GetUserMiniDTOAsync(found.AdminId)
            };
            return Ok(mapped);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddBanDTO banDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var mapped = _mapper.Map<BanModel>(banDTO);

            var adminId = User.GetId();
            if (adminId is null)
                return Forbid();
            if (adminId.Value == banDTO.UserId)
                return BadRequest("You cannot ban yourself");
            mapped.AdminId = adminId.Value;
            BanDTO mappedCreated;
            try
            {
                await _banService.AddAsync(mapped);
                await _refreshTokenService.DeleteUserTokensAsync(mapped.UserId);
                mappedCreated = _mapper.Map<BanDTO>(mapped);
                mappedCreated = mappedCreated with
                {
                    User = (await GetUserMiniDTOAsync(mapped.UserId))!,
                    Admin = await GetUserMiniDTOAsync(mapped.AdminId)
                };

                var connectionIds = Request.Headers["ConnectionId"];
                await _hubContext.Clients.GroupExcept(mapped.UserId.ToString(), connectionIds)
                    .SendAsync("AddedBan", mapped.UserId, mappedCreated);

                await _usersHubContext.Clients.User(mapped.UserId.ToString()).SendAsync("Banned", mappedCreated);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }
            return Created($"{this.GetApiUrl()}Bans/{mapped.Id}", mappedCreated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var found = await _banService.GetByIdAsync(id);
            if (found is null)
                return NotFound();
            try
            {
                await _banService.DeleteByIdAsync(id);

                var connectionIds = Request.Headers["ConnectionId"];
                await _hubContext.Clients.GroupExcept(found.UserId.ToString(), connectionIds)
                    .SendAsync("DeletedBan", found.UserId, id);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
