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
                .Include(t => t.Executors)
                .SingleOrDefaultAsync(t => t.Id == id);
            if (model is null)
                throw new InvalidOperationException("Model with such an id was not found");
            return model;
        }
        public async Task AddAsync(TaskModel model)
        {
            if (model.StartDate == default)
                model.StartDate = DateTime.UtcNow;
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
            var mapped = _mapper.Map<TaskEntity>(model);
            await _context.Tasks.AddAsync(mapped);
            await _context.SaveChangesAsync();
            model.Id = mapped.Id;
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
            if (model.Status != TaskStatuses.Archived)
                throw new InvalidOperationException("Archive a task first");
            var files = model.Files.ToList();
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
            var tagExtended = await _context.Tags.Include(t => t.Tasks).SingleOrDefaultAsync(t => t.Id == tagId);
            if (tagExtended is not null && !tagExtended.Tasks.Any())
            {
                _context.Tags.Remove(tagExtended);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<TaskModel?> GetByIdAsync(int id)
        {
            var model = await _context.Tasks
                .Include(t => t.Messages)
                .Include(t => t.Files)
                .Include(t => t.Tags)
                .Include(t => t.Executors)
                .SingleOrDefaultAsync(m => m.Id == id);
            return _mapper.Map<TaskModel?>(model);
        }

        public IEnumerable<TaskModel> GetUserTasks(int userId)
        {
            return _mapper.Map<IEnumerable<TaskModel>>(_context.Tasks
                .Include(t => t.Executors))
                .Where(t => t.ExecutorsIds.Contains(userId))
                .OrderBy(t => IsDone(t.Status))
                .ThenByDescending(t => t.Id);
        }

        internal static bool IsDone(TaskStatuses status) => status >= TaskStatuses.Validate;

        public bool IsValid(TaskModel model, out string? firstErrorMessage)
        {
            var result = IModelValidator<TaskModel>.IsValidByDefault(model, out firstErrorMessage);
            if (!result)
                return false;
            if ((model.Deadline is not null && model.Deadline <= model.StartDate) 
                || (model.EndDate is not null && model.EndDate < model.StartDate))
            {
                firstErrorMessage = "Something wrong with start date, deadline or end date";
                return false;
            }
            if (!_context.Projects.Any(p => p.Id == model.ProjectId))
            {
                firstErrorMessage = "The project with such an id was not found";
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
            if (model.Status == TaskStatuses.Complete && existingModel.Status < TaskStatuses.Complete)
                model.EndDate = DateTime.UtcNow;
            else if (model.Status < TaskStatuses.Complete)
                model.EndDate = null;
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

        public IEnumerable<TaskModel> GetProjectTasksByStatusAndTag(int projectId, TaskStatuses status, int? tagId = null)
        {
            var tasksExtended = _context.Tasks
                .Include(t => t.Messages)
                .Include(t => t.Files)
                .Include(t => t.Tags);
            if (tagId is null)
                return _mapper.Map<IEnumerable<TaskModel>>(tasksExtended.Where(t => t.ProjectId == projectId && t.Status == status));
            var result = tasksExtended
                .Include(t => t.Tags)
                .Where(t => t.ProjectId == projectId && t.Status == status && t.Tags.Any(t => t.Id == tagId));
            return _mapper.Map<IEnumerable<TaskModel>>(result);
        }

        public async Task<IEnumerable<User>> GetTaskExecutorsAsync(int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.Executors)
                .SingleOrDefaultAsync(t => t.Id == taskId);
            return (task is null) ? new List<User>() : task.Executors;
        }

        const int _maxExecutorsCount = 10;

        public async Task AddExecutorToTaskAsync(int taskId, int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user is null)
                throw new InvalidOperationException("User with such an id was not found");
            var task = await GetNotMappedByIdAsync(taskId);
            if (task.Executors.Any(t => t.Id == userId))
                throw new InvalidOperationException("This task already has such a user");
            if (!_context.UsersOnProjects.Any(uop => uop.UserId == userId && uop.ProjectId == task.ProjectId))
                throw new ArgumentException("The user with such an id (ExecutorId) does not work on this project",
                    nameof(userId));
            if (task.Executors.Count >= _maxExecutorsCount)
                throw new InvalidOperationException($"Too many executors. Maximum: {_maxExecutorsCount}");
            task.Executors.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteExecutorFromTaskAsync(int taskId, int userId)
        {
            var task = await GetNotMappedByIdAsync(taskId);
            var user = task.Executors.SingleOrDefault(t => t.Id == userId);
            if (user is null)
                throw new InvalidOperationException("User with such an id does not belong to the task");
            task.Executors.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
