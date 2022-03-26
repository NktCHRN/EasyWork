using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;
using TaskEntity = Data.Entities.Task;

namespace Business.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;

        private readonly IMapper _mapper;

        public TaskService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private async Task<TaskEntity> GetNotMappedByIdAsync(int id)
        {
            var model = await _context.Tasks
                .Include(t => t.Tags)
                .SingleOrDefaultAsync(t => t.Id == id);
            if (model is null)
                throw new InvalidOperationException("Model with such an id was not found");
            return model;
        }
        public async Task AddAsync(TaskModel model)
        {
            if (model.StartDate == default)
                model.StartDate = DateTime.Now;
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            if (model.EndDate is not null)
                throw new ArgumentException("EndDate should be null on creation", nameof(model)); 
            await _context.Tasks.AddAsync(_mapper.Map<TaskEntity>(model));
            await _context.SaveChangesAsync();
        }

        public async Task AddTagToTaskAsync(int taskId, int tagId)
        {
            var tag = await _context.Tags.FindAsync(tagId);
            if (tag is null)
                throw new InvalidOperationException("Tag with such an id was not found");
            var task = await GetNotMappedByIdAsync(taskId);
            if (tag.ProjectId != task.ProjectId)
                throw new InvalidOperationException("Tag should belong to the same project task belongs to");
            if (task.Tags.Any(t => t.Id == tagId))
                throw new InvalidOperationException("This task already has such a tag");
            task.Tags.Add(tag);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(int id)
        {
            var model = await GetNotMappedByIdAsync(id);
            _context.Tasks.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTagFromTaskAsync(int taskId, int tagId)
        {
            var task = await GetNotMappedByIdAsync(taskId);
            var tag = task.Tags.SingleOrDefault(t => t.Id == tagId);
            if (tag is null)
                throw new InvalidOperationException("Tag with such an id does not belong to the task");
            task.Tags.Remove(tag);
            await _context.SaveChangesAsync();
        }

        public async Task<TaskModel> GetByIdAsync(int id)
        {
            var model = await _context.Tasks
                .Include(t => t.Messages)
                .Include(t => t.Files)
                .Include(t => t.Tags)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (model is null)
                throw new InvalidOperationException("Model with such an id was not found");
            return _mapper.Map<TaskModel>(model);
        }

        public IEnumerable<TaskModel> GetProjectNotArchivedTasks(int projectId)
        {
            return _mapper.Map<IEnumerable<TaskModel>>(_context.Tasks
                .Where(t => t.ProjectId == projectId && t.Status != Data.Entities.TaskStatuses.Archived)).Reverse();
        }

        public IEnumerable<TaskModel> GetProjectArchivedTasks(int projectId)
        {
            return _mapper.Map<IEnumerable<TaskModel>>(_context.Tasks
                .Where(t => t.ProjectId == projectId && t.Status == Data.Entities.TaskStatuses.Archived)).Reverse();
        }

        public IEnumerable<TaskModel> GetUserTasks(int userId)
        {
            return _mapper.Map<IEnumerable<TaskModel>>(_context.Tasks
                .Where(t => t.ExecutorId == userId)).OrderBy(t => IsDone(t.Status)).ThenByDescending(t => t.Id);
        }

        internal static bool IsDone(TaskStatuses status) => status >= TaskStatuses.Validate;

        public bool IsValid(TaskModel model, out string? firstErrorMessage)
        {
            var result = IModelValidator<TaskModel>.IsValidByDefault(model, out firstErrorMessage);
            if (!result)
                return false;
            if ((model.Deadline is not null && model.Deadline <= model.StartDate) 
                || (model.EndDate is not null && model.Deadline is not null && model.EndDate < model.Deadline))
            {
                firstErrorMessage = "Something wrong with start date, deadline or end date";
                return false;
            }
            if (!_context.Projects.Any(p => p.Id == model.ProjectId))
            {
                firstErrorMessage = "The project with such an id was not found";
                return false;
            }
            if (model.ExecutorId is not null && !_context.Users.Any(p => p.Id == model.ExecutorId))
            {
                firstErrorMessage = "The user with such an id (ExecutorId) was not found";
                return false;
            }
            return true;
        }

        public async Task UpdateAsync(TaskModel model)
        {
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            var existingModel = await GetNotMappedByIdAsync(model.Id);
            if (model.ProjectId != existingModel.ProjectId)
                throw new ArgumentException("Project id cannot be changed", nameof(model));
            existingModel = _mapper.Map(model, existingModel);
            _context.Tasks.Update(existingModel);
            await _context.SaveChangesAsync();
        }

        public IEnumerable<TaskModel> GetProjectTasksByDate(int projectId, DateTime from, DateTime to)
        {
            return _mapper.Map<IEnumerable<TaskModel>>(_context.Tasks
                .Where(t => t.ProjectId == projectId 
                    && !(t.EndDate != null && t.EndDate <= from)
                    && !(t.StartDate >= to)));
        }
    }
}
