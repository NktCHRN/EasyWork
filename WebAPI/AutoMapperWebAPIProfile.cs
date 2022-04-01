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
            CreateMap<BanModel, BannedUserDTO>()
                .ForMember(u => u.AdminName, m => m.Ignore())
                .ForMember(u => u.AdminEmail, m => m.Ignore());
            CreateMap<UpdateUserDTO, User>();
            CreateMap<User, UserCabinetProfileDTO>()
                .ForMember(u => u.MIMEAvatarType, m => m.Ignore())
                .ForMember(u => u.Projects, m => m.Ignore())
                .ForMember(u => u.TasksDone, m => m.Ignore())
                .ForMember(u => u.TasksNotDone, m => m.Ignore())
                .ForMember(u => u.Avatar, m => m.Ignore());
            CreateMap<User, UserProfileDTO>()
                .ForMember(u => u.LastSeen,
                    m => m.MapFrom<DateTime?>(usr => (usr.LastSeen == DateTime.MinValue) ? null : usr.LastSeen))
                .ForMember(u => u.MIMEAvatarType, m => m.Ignore())
                .ForMember(u => u.Projects, m => m.Ignore())
                .ForMember(u => u.TasksDone, m => m.Ignore())
                .ForMember(u => u.TasksNotDone, m => m.Ignore())
                .ForMember(u => u.Avatar, m => m.Ignore());
            CreateMap<User, UserProfileReducedDTO>()
                .ForMember(u => u.LastSeen,
                    m => m.MapFrom<DateTime?>(usr => (usr.LastSeen == DateTime.MinValue) ? null : usr.LastSeen))
                .ForMember(u => u.FullName,
                    m => m.MapFrom<string>(usr => 
                        string.IsNullOrEmpty(usr.LastName) ? usr.FirstName : usr.FirstName + " " + usr.LastName))
                .ForMember(u => u.MIMEAvatarType, m => m.Ignore())
                .ForMember(u => u.Avatar, m => m.Ignore());
        }
    }
}
