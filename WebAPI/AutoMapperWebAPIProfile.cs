﻿using AutoMapper;
using Business.Models;
using Business.Other;
using Data.Entities;
using WebAPI.DTOs.Ban;
using WebAPI.DTOs.File;
using WebAPI.DTOs.Message;
using WebAPI.DTOs.Project;
using WebAPI.DTOs.Project.InviteCode;
using WebAPI.DTOs.Project.Limits;
using WebAPI.DTOs.Task;
using WebAPI.DTOs.User;
using WebAPI.DTOs.User.Profile;
using WebAPI.DTOs.UserOnProject;

namespace WebAPI
{
    public class AutoMapperWebAPIProfile : Profile
    {
        public AutoMapperWebAPIProfile()
        {
            CreateMap<RegisterUserDTO, User>()
                .ForSourceMember(s => s.Password, opt => opt.DoNotValidate())
                .ForSourceMember(s => s.PasswordConfirm, opt => opt.DoNotValidate())
                .ForMember(u => u.FirstName, s => s.MapFrom(r => r.FirstName.Trim()))
                .ForMember(u => u.LastName, s => s.MapFrom(r => 
                    string.IsNullOrWhiteSpace(r.LastName) ? null : r.LastName.Trim()));
            CreateMap<BanModel, BannedUserDTO>()
                .ForMember(u => u.Admin, m => m.Ignore());
            CreateMap<BanModel, BanDTO>()
                .ForMember(u => u.Admin, m => m.Ignore())
                .ForMember(u => u.User, m => m.Ignore());
            CreateMap<AddBanDTO, BanModel>();
            CreateMap<UpdateUserDTO, User>()
                .ForMember(u => u.FirstName, s => s.MapFrom(r => r.FirstName.Trim()))
                .ForMember(u => u.LastName, s => s.MapFrom(r =>
                    string.IsNullOrWhiteSpace(r.LastName) ? null : r.LastName.Trim()));
            CreateMap<User, UserCabinetProfileDTO>()
                .ForMember(u => u.MIMEAvatarType, m => m.Ignore())
                .ForMember(u => u.Projects, m => m.Ignore())
                .ForMember(u => u.TasksDone, m => m.Ignore())
                .ForMember(u => u.TasksNotDone, m => m.Ignore())
                .ForMember(u => u.AvatarURL, m => m.Ignore());
            CreateMap<User, UserProfileDTO>()
                .ForMember(u => u.LastSeen,
                    m => m.MapFrom(usr => (usr.LastSeen == DateTimeOffset.MinValue) ? null : usr.LastSeen))
                .ForMember(u => u.MIMEAvatarType, m => m.Ignore())
                .ForMember(u => u.Projects, m => m.Ignore())
                .ForMember(u => u.TasksDone, m => m.Ignore())
                .ForMember(u => u.TasksNotDone, m => m.Ignore())
                .ForMember(u => u.AvatarURL, m => m.Ignore());
            CreateMap<User, UserProfileReducedDTO>()
                .ForMember(u => u.LastSeen,
                    m => m.MapFrom(usr => (usr.LastSeen == DateTimeOffset.MinValue) ? null : usr.LastSeen))
                .ForMember(u => u.FullName,
                    m => m.MapFrom(usr => $"{usr.FirstName} {usr.LastName}".TrimEnd()))
                .ForMember(u => u.MIMEAvatarType, m => m.Ignore())
                .ForMember(u => u.AvatarURL, m => m.Ignore());
            CreateMap<UserOnProjectModel, UserOnProjectDTO>()
                .ForMember(u => u.Role, m => m.MapFrom(usr => usr.Role.ToString()));
            CreateMap<UserOnProjectModel, UserOnProjectMiniDTO>();
            CreateMap<UserOnProjectModel, UserOnProjectReducedDTO>()
                .ForMember(u => u.Role, m => m.MapFrom(usr => usr.Role.ToString()));
            CreateMap<ProjectModel, ProjectReducedDTO>();
            CreateMap<ProjectModel, ProjectDTO>();
            CreateMap<ProjectModel, InviteCodeDTO>();
            CreateMap<UpdateProjectDTO, ProjectModel>();
            CreateMap<UpdateInviteCodeStatusDTO, ProjectModel>()
                .ForMember(d => d.IsInviteCodeActive, sc => sc.MapFrom(s => s.IsActive));
            CreateMap<TaskModel, TaskReducedDTO>()
                .ForMember(t => t.MessagesCount, m => m.MapFrom(tsk => tsk.MessagesIds.Count))
                .ForMember(t => t.FilesCount, m => m.MapFrom(tsk => tsk.FilesIds.Count))
                .ForMember(t => t.Priority, m => m.MapFrom(tsk => (tsk.Priority == null) ? null : tsk.Priority.Value.ToString()));
            CreateMap<TaskModel, TaskDTO>()
                .ForMember(t => t.Status, m => m.MapFrom(tsk => tsk.Status.ToString()))
                .ForMember(t => t.Priority, m => m.MapFrom(tsk => (tsk.Priority == null) ? null : tsk.Priority.Value.ToString()));
            CreateMap<TaskModel, UserTaskDTO>()
                .ForMember(t => t.Status, m => m.MapFrom(tsk => tsk.Status.ToString()));
            CreateMap<UpdateTaskDTO, TaskModel>()
                .ForSourceMember(t => t.Status, m => m.DoNotValidate())
                .ForSourceMember(t => t.Priority, m => m.DoNotValidate());
            ValueTransformers.Add<byte[]?>(val => (val == null || val.Length == 0) ? null : val);
            CreateMap<FileModelExtended, FileDTO>();
            CreateMap<MessageModel, MessageDTO>()
                .ForMember(m => m.Sender, u => u.Ignore());
            CreateMap<ProjectLimitsModel, ProjectLimitsDTO>()
                .ReverseMap();
            CreateMap<FileModel, FileReducedDTO>();
        }
    }
}
