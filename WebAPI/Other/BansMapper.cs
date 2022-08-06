using AutoMapper;
using Business.Models;
using Data.Entities;
using Microsoft.AspNetCore.Identity;
using WebAPI.DTOs.User;
using WebAPI.Interfaces;

namespace WebAPI.Other
{
    public class BansMapper : ICustomAsyncMapper<IEnumerable<BanModel>, IEnumerable<BannedUserDTO>>
    {
        private readonly UserManager<User> _userManager;

        private readonly IMapper _mapper;

        public BansMapper(IMapper mapper, UserManager<User> userManager)
        {
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<IEnumerable<BannedUserDTO>?> MapAsync(IEnumerable<BanModel>? model)
        {
            var mappedBans = new List<BannedUserDTO>();
            if (model is null)
                return mappedBans;
            foreach (var ban in model)
            {
                var mappedBan = _mapper.Map<BannedUserDTO>(ban);
                var admin = await _userManager.FindByIdAsync(ban.AdminId.GetValueOrDefault().ToString());
                if (admin is not null)
                {
                    var adminModel = new UserMiniDTO()
                    {
                        Id = admin.Id,
                        Email = admin.Email,
                        FullName = $"{admin.FirstName} {admin.LastName}".TrimEnd(),
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
    }
}
