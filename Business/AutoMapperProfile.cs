using AutoMapper;
using Business.Models;
using Data.Entities;

namespace Business
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Ban, BanModel>().ReverseMap();
            CreateMap<Data.Entities.File, FileModel>().ReverseMap();
            CreateMap<Message, MessageModel>()
                .ForMember(e => e.FilesIds, 
                m => m.MapFrom(mm => mm.Files.Select(f => f.Id)))
                .ReverseMap();
            CreateMap<Project, ProjectModel>()
                .ForMember(e => e.TeamMembersIds,
                m => m.MapFrom(pm => pm.TeamMembers.Select(t => new Tuple<int, int>(t.ProjectId, t.UserId))))
                .ForMember(e => e.ReleasesIds,
                m => m.MapFrom(pm => pm.Releases.Select(r => r.Id)))
                .ForMember(e => e.TasksIds,
                m => m.MapFrom(pm => pm.Tasks.Select(t => t.Id)))
                .ReverseMap();
            CreateMap<Release, ReleaseModel>().ReverseMap();
            CreateMap<Data.Entities.Task, TaskModel>()
                .ForMember(e => e.Files,
                m => m.MapFrom(tm => tm.Files.Select(f => f.Id)))
                .ForMember(e => e.Messages,
                m => m.MapFrom(tm => tm.Messages.Select(msg => msg.Id)))
                .ReverseMap();
            CreateMap<User, UserModel>()
                .ForMember(e => e.BansIds,
                m => m.MapFrom(um => um.Bans.Select(b => b.Id)))
                .ForMember(e => e.GivenBansIds,
                m => m.MapFrom(um => um.GivenBans.Select(gb => gb.Id)))
                .ForMember(e => e.OwnedProjectsIds,
                m => m.MapFrom(um => um.OwnedProjects.Select(op => op.Id)))
                .ForMember(e => e.ProjectsIds,
                m => m.MapFrom(um => um.Projects.Select(p => new Tuple<int, int>(p.ProjectId, p.UserId))))
                .ForMember(e => e.TasksIds,
                m => m.MapFrom(um => um.Tasks.Select(t => t.Id)))
                .ForMember(e => e.MessagesIds,
                m => m.MapFrom(um => um.Messages.Select(m => m.Id)))
                .ReverseMap();
            CreateMap<UserOnProject, UserOnProjectModel>().ReverseMap();
        }
    }
}
