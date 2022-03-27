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

        private readonly IFileManager _manager;

        public TaskService(ApplicationDbContext context, IMapper mapper, IFileManager manager)
        {
            _context = context;
            _mapper = mapper;
            _manager = manager;
        }

        private async Task<TaskEntity> GetNotMappedByIdAsync(int id)
        {
            var model = await _context.Tasks
                .Include(t => t.Tags)
                .Include(t => t.Files)
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
            var project = await _context.Projects.SingleAsync(p => p.Id == model.ProjectId);
            switch (model.Status)
            {
                case TaskStatuses.ToDo:
                    if (project.MaxToDo is not null &&
                        _context.Tasks.Where(t => t.ProjectId == model.ProjectId && t.Status == TaskStatuses.ToDo).Count()
                        >= project.MaxToDo)
                        throw new InvalidOperationException("You cannot exceed the \"ToDo\" tasks limit");
                    break;
                case TaskStatuses.InProgress:
                    if (project.MaxInProgress is not null &&
                        _context.Tasks.Where(t => t.ProjectId == model.ProjectId && t.Status == TaskStatuses.InProgress).Count()
                        >= project.MaxInProgress)
                        throw new InvalidOperationException("You cannot exceed the \"InProgress\" tasks limit");
                    break;
                case TaskStatuses.Validate:
                    if (project.MaxValidate is not null &&
                        _context.Tasks.Where(t => t.ProjectId == model.ProjectId && t.Status == TaskStatuses.Validate).Count()
                        >= project.MaxValidate)
                        throw new InvalidOperationException("You cannot exceed the \"Validate\" tasks limit");
                    break;
            }
            await _context.Tasks.AddAsync(_mapper.Map<TaskEntity>(model));
            await _context.SaveChangesAsync();
        }

        public async Task AddTagToTaskAsync(int taskId, int tagId)
        {
            var tag = await _context.Tags.FindAsync(tagId);
            if (tag is null)
                throw new InvalidOperationException("Tag with such an id was not found");
            var task = await GetNotMappedByIdAsync(taskId);
            if (task.Tags.Any(t => t.Id == tagId))
                throw new InvalidOperationException("This task already has such a tag");
            task.Tags.Add(tag);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(int id)
        {
            var model = await GetNotMappedByIdAsync(id);
            var files = model.Files.ToList();
            var messages = _context.Messages.Include(m => m.Files).Where(m => m.TaskId == id);
            foreach (var message in messages)
                files.AddRange(message.Files);
            foreach (var file in files)
            {
                _context.Files.Remove(file);
                await _context.SaveChangesAsync();
                try
                {
                    _manager.DeleteFile(file.Id.ToString() + Path.GetExtension(file.Name), Enums.EasyWorkFileTypes.File);
                }
                catch (Exception) { }
            }
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
