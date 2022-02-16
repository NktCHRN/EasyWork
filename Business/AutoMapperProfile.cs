﻿using AutoMapper;
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
                .ForMember(e => e.TagsIds,
                m => m.MapFrom(pm => pm.Tags.Select(t => t.Id)))
                .ReverseMap();
            CreateMap<Release, ReleaseModel>().ReverseMap();
            CreateMap<Data.Entities.Task, TaskModel>()
                .ForMember(e => e.Files,
                m => m.MapFrom(tm => tm.Files.Select(f => f.Id)))
                .ForMember(e => e.Messages,
                m => m.MapFrom(tm => tm.Messages.Select(msg => msg.Id)))
                .ForMember(e => e.TagsIds,
                m => m.MapFrom(taskm => taskm.Tags.Select(t => t.Id)))
                .ReverseMap();
            CreateMap<Tag, TagModel>()
                .ForMember(e => e.TasksIds,
                m => m.MapFrom(tagm => tagm.Tasks.Select(t => t.Id)))
                .ReverseMap();
            CreateMap<UserOnProject, UserOnProjectModel>().ReverseMap();
        }
    }
}
