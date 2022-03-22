﻿using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Business.Services
{
    public class TagService : ITagService
    {
        private readonly ApplicationDbContext _context;

        private readonly IMapper _mapper;

        public TagService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private async Task<Tag> GetNotMappedByIdAsync(int id)
        {
            var model = await _context.Tags.FindAsync(id);
            if (model is null)
                throw new InvalidOperationException("Model with such an id was not found");
            return model;
        }

        public async Task AddAsync(TagModel model)
        {
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            await _context.Tags.AddAsync(_mapper.Map<Tag>(model));
            await _context.SaveChangesAsync(); ;
        }

        public async Task DeleteByIdAsync(int id)
        {
            var model = await GetNotMappedByIdAsync(id);
            _context.Tags.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task<TagModel> GetByIdAsync(int id)
        {
            var model = await _context.Tags
                .Include(t => t.Tasks)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (model is null)
                throw new InvalidOperationException("Model with such an id was not found");
            return _mapper.Map<TagModel>(model);
        }

        public IEnumerable<TagModel> GetProjectTags(int projectId)
        {
            return _mapper.Map<IEnumerable<TagModel>>(_context.Tags.Where(t => t.ProjectId == projectId));
        }

        public async Task<IEnumerable<TagModel>> GetTaskTagsAsync(int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.Tags)
                .SingleOrDefaultAsync(t => t.Id == taskId);
            return (task is null) ? new List<TagModel>() : _mapper.Map<IEnumerable<TagModel>>(task.Tags);
        }

        public bool IsValid(TagModel model, out string? firstErrorMessage)
        {
            var result = IModelValidator<TagModel>.IsValidByDefault(model, out firstErrorMessage);
            if (!result)
                return false;
            if (!_context.Projects.Any(p => p.Id == model.ProjectId))
            {
                firstErrorMessage = "The project with such an id was not found";
                return false;
            }
            return true;
        }

        public async Task UpdateAsync(TagModel model)
        {
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            var existingModel = await GetNotMappedByIdAsync(model.Id);
            if (model.ProjectId != existingModel.ProjectId)
                throw new ArgumentException("Project id cannot be changed", nameof(model));
            existingModel = _mapper.Map(model, existingModel);
            _context.Tags.Update(existingModel);
            await _context.SaveChangesAsync();
        }
    }
}
