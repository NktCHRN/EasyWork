using AutoMapper;
using Business.Models;
using Data.Entities;
using WebAPI.DTOs;

namespace WebAPI
{
    public class AutoMapperWebAPIProfile : Profile
    {
        public AutoMapperWebAPIProfile()
        {
            CreateMap<RegisterUserDTO, User>()
                .ForSourceMember(s => s.Password, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.PasswordConfirm, opt => opt.DoNotValidate());
            CreateMap<BanModel, BannedUserDTO>();
            CreateMap<UpdateUserDTO, User>();
        }
    }
}
