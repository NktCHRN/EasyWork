using AutoMapper;
using Business.Models;
using Business.Other;
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
                .ForMember(u => u.Admin, m => m.Ignore());
            CreateMap<BanModel, BanDTO>()
                .ForMember(u => u.Admin, m => m.Ignore())
                .ForMember(u => u.User, m => m.Ignore());
            CreateMap<AddBanDTO, BanModel>();
            CreateMap<UpdateUserDTO, User>();
            CreateMap<User, UserCabinetProfileDTO>()
                .ForMember(u => u.MIMEAvatarType, m => m.Ignore())
                .ForMember(u => u.Projects, m => m.Ignore())
                .ForMember(u => u.TasksDone, m => m.Ignore())
                .ForMember(u => u.TasksNotDone, m => m.Ignore())
                .ForMember(u => u.AvatarURL, m => m.Ignore());
            CreateMap<User, UserProfileDTO>()
                .ForMember(u => u.LastSeen,
                    m => m.MapFrom<DateTime?>(usr => (usr.LastSeen == DateTime.MinValue) ? null : usr.LastSeen))
                .ForMember(u => u.MIMEAvatarType, m => m.Ignore())
                .ForMember(u => u.Projects, m => m.Ignore())
                .ForMember(u => u.TasksDone, m => m.Ignore())
                .ForMember(u => u.TasksNotDone, m => m.Ignore())
                .ForMember(u => u.AvatarURL, m => m.Ignore());
            CreateMap<User, UserProfileReducedDTO>()
                .ForMember(u => u.LastSeen,
                    m => m.MapFrom<DateTime?>(usr => (usr.LastSeen == DateTime.MinValue) ? null : usr.LastSeen))
                .ForMember(u => u.FullName,
                    m => m.MapFrom(usr => $"{usr.FirstName} {usr.LastName}".TrimEnd()))
                .ForMember(u => u.MIMEAvatarType, m => m.Ignore())
                .ForMember(u => u.AvatarURL, m => m.Ignore());
            CreateMap<UserOnProjectModel, UserOnProjectDTO>()
                .ForMember(u => u.Role, m => m.MapFrom(usr => usr.Role.ToString()));
            CreateMap<ProjectModel, ProjectReducedDTO>();
            CreateMap<ProjectModel, ProjectDTO>();
            CreateMap<UpdateProjectDTO, ProjectModel>();
            CreateMap<ReleaseModel, ReleaseDTO>();
            CreateMap<AddReleaseDTO, ReleaseModel>();
            CreateMap<TagModel, TagDTO>();
            CreateMap<TaskModel, TaskReducedDTO>()
                .ForMember(t => t.Executor, m => m.Ignore())
                .ForMember(t => t.MessagesCount, m => m.MapFrom(tsk => tsk.MessagesIds.Count))
                .ForMember(t => t.FilesCount, m => m.MapFrom(tsk => tsk.FilesIds.Count));
            CreateMap<TaskModel, TaskDTO>()
                .ForMember(t => t.Executor, m => m.Ignore())
                .ForMember(t => t.Status, m => m.MapFrom(tsk => tsk.Status.ToString()))
                .ForMember(t => t.Priority, m => m.MapFrom(tsk => (tsk.Priority == null) ? null : tsk.Priority.Value.ToString()));
            CreateMap<TaskModel, UserTaskDTO>()
                .ForMember(t => t.Status, m => m.MapFrom(tsk => tsk.Status.ToString()));
            CreateMap<UpdateTaskDTO, TaskModel>()
                .ForSourceMember(t => t.Status, m => m.DoNotValidate())
                .ForSourceMember(t => t.Priority, m => m.DoNotValidate());
            ValueTransformers.Add<byte[]?>(val => (val == null || val.Length == 0) ? null : val);
            CreateMap<FileModelExtended, FileModelDTO>();
            CreateMap<MessageModel, MessageDTO>()
                .ForMember(m => m.Sender, u => u.Ignore());
        }
    }
}
