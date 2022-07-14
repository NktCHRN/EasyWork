using AutoMapper;
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
            var model = await _context.Tags
                .Include(t => t.Tasks)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (model is null)
                throw new InvalidOperationException("Model with such an id was not found");
            return model;
        }

        public async Task AddAsync(TagModel model)
        {
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            var mapped = _mapper.Map<Tag>(model);
            await _context.Tags.AddAsync(mapped);
            await _context.SaveChangesAsync();
            model.Id = mapped.Id;
        }

        public async Task DeleteByIdAsync(int id)
        {
            var model = await GetNotMappedByIdAsync(id);
            _context.Tags.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task<TagModel?> GetByIdAsync(int id)
        {
            var model = await _context.Tags
                .Include(t => t.Tasks)
                .SingleOrDefaultAsync(m => m.Id == id);
            return _mapper.Map<TagModel?>(model);
        }

        public IEnumerable<TagModel> GetProjectTags(int projectId)
        {
            return _mapper.Map<IEnumerable<TagModel>>(_context.Tags.Include(t => t.Tasks)
                .Where(t => t.Tasks.Any(task => task.ProjectId == projectId)))
                .OrderBy(t => t.Name);
        }

        public async Task<IEnumerable<TagModel>> GetTaskTagsAsync(int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.Tags)
                .SingleOrDefaultAsync(t => t.Id == taskId);
            return (task is null) ? new List<TagModel>() : _mapper.Map<IEnumerable<TagModel>>(task.Tags).Reverse();
        }

        public bool IsValid(TagModel model, out string? firstErrorMessage)
        {
            var result = IModelValidator<TagModel>.IsValidByDefault(model, out firstErrorMessage);
            if (!result)
                return false;
            if (model.Name.ToLowerInvariant() == "all")
            {
                firstErrorMessage = "Tag name cannot be \"All\" case-insensitively";
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
            existingModel = _mapper.Map(model, existingModel);
            _context.Tags.Update(existingModel);
            await _context.SaveChangesAsync();
        }

        public async Task<TagModel?> FindByName(string name) => 
            _mapper.Map<TagModel?>(await _context.Tags.FirstOrDefaultAsync(t => t.Name == name));

        public async Task DeleteFromProjectByIdAsync(int id, int projectId)
        {
            var model = await GetNotMappedByIdAsync(id);
            var projectTasks = model.Tasks.Where(t => t.ProjectId == projectId).ToList();
            if (!projectTasks.Any())
                throw new InvalidOperationException("This tag does not belongs to the project");
            foreach (var task in projectTasks)
            {
                model.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
            if (!model.Tasks.Any())
            {
                _context.Tags.Remove(model);
                await _context.SaveChangesAsync();
            }
        }
    }
}
