using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebAPI.DTOs;
using WebAPI.Other;

namespace WebAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class BansController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public readonly IBanService _banService;

        private readonly IMapper _mapper;

        public BansController(IMapper mapper, UserManager<User> userManager, IBanService banService)
        {
            _mapper = mapper;
            _userManager = userManager;
            _banService = banService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLastBans([FromQuery] int? quantity)
        {
            int count = (quantity is null) ? int.MaxValue : quantity.Value;
            var bans = _banService.GetLast(count);
            var banDTOs = new List<BanDTO>();
            foreach (var ban in bans)
            {
                var mapped = _mapper.Map<BanDTO>(ban);
                mapped = mapped with
                {
                    User = (await GetUserMiniDTOAsync(ban.UserId))!,
                    Admin = await GetUserMiniDTOAsync(ban.AdminId)
                };
                banDTOs.Add(mapped);
            }
            return Ok(banDTOs);
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
                    FullName = (user.LastName is null) ? user.FirstName : user.FirstName + " " + user.LastName,
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

            try
            {
                await _banService.AddAsync(mapped);
            }
            catch (ArgumentException exc)
            {
                return BadRequest(exc.Message);
            }

            var mappedCreated = _mapper.Map<BanDTO>(mapped);
            mappedCreated = mappedCreated with
            {
                User = (await GetUserMiniDTOAsync(mapped.UserId))!,
                Admin = await GetUserMiniDTOAsync(mapped.AdminId)
            };
            return Created($"{this.GetApiUrl()}Bans/{mapped.Id}", mappedCreated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var found = await _banService.GetByIdAsync(id);
            if (found is null)
                return NotFound();
            await _banService.DeleteByIdAsync(id);
            return NoContent();
        }
    }
}
