using AutoMapper;
using Business.Models;
using Business.Other;
using Data.Entities;

namespace Business
{
    public class AutoMapperBusinessProfile : Profile
    {
        public AutoMapperBusinessProfile()
        {
            CreateMap<Ban, BanModel>().ReverseMap();
            CreateMap<Data.Entities.File, FileModel>().ReverseMap();
            CreateMap<Message, MessageModel>()
                .ReverseMap();
            CreateMap<Project, ProjectModel>()
                .ForMember(e => e.TeamMembersIds,
                m => m.MapFrom(pm => pm.TeamMembers.Select(t => ValueTuple.Create(t.ProjectId, t.UserId))))
                .ForMember(e => e.ReleasesIds,
                m => m.MapFrom(pm => pm.Releases.Select(r => r.Id)))
                .ForMember(e => e.TasksIds,
                m => m.MapFrom(pm => pm.Tasks.Select(t => t.Id)))
                .ForMember(e => e.Limits,
                m => m.MapFrom(pm => new ProjectLimitsModel
                {
                    MaxToDo = pm.MaxToDo,
                    MaxInProgress = pm.MaxInProgress,
                    MaxValidate = pm.MaxValidate
                }));
            CreateMap<ProjectModel, Project>()
                .ForMember(d => d.MaxToDo,
                s => s.MapFrom(m => m.Limits.MaxToDo))
                .ForMember(d => d.MaxInProgress,
                s => s.MapFrom(m => m.Limits.MaxInProgress))
                .ForMember(d => d.MaxValidate,
                s => s.MapFrom(m => m.Limits.MaxValidate));
            CreateMap<ProjectLimitsModel, Project>()
                .ForMember(d => d.MaxToDo,
                s => s.MapFrom(m => m.MaxToDo))
                .ForMember(d => d.MaxInProgress,
                s => s.MapFrom(m => m.MaxInProgress))
                .ForMember(d => d.MaxValidate,
                s => s.MapFrom(m => m.MaxValidate));
            CreateMap<Release, ReleaseModel>().ReverseMap();
            CreateMap<Data.Entities.Task, TaskModel>()
                .ForMember(e => e.FilesIds,
                m => m.MapFrom(tm => tm.Files.Select(f => f.Id)))
                .ForMember(e => e.MessagesIds,
                m => m.MapFrom(tm => tm.Messages.Select(msg => msg.Id)))
                .ForMember(e => e.TagsIds,
                m => m.MapFrom(taskm => taskm.Tags.Select(t => t.Id)))
                .ForMember(e => e.ExecutorsIds,
                m => m.MapFrom(tm => tm.Executors.Select(ex => ex.Id)))
                .ReverseMap();
            CreateMap<Tag, TagModel>()
                .ForMember(e => e.TasksIds,
                m => m.MapFrom(tagm => tagm.Tasks.Select(t => t.Id)))
                .ReverseMap();
            CreateMap<UserOnProject, UserOnProjectModel>().ReverseMap();
            CreateMap<Data.Entities.File, FileModelExtended>()
                .ForMember(f => f.Size, m => m.Ignore());
            CreateMap<RefreshToken, RefreshTokenModel>().ReverseMap();
        }
    }
}
