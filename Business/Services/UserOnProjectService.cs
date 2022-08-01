using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Business.Other;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Business.Services
{
    public class UserOnProjectService : IUserOnProjectService
    {
        private readonly ApplicationDbContext _context;

        private readonly IMapper _mapper;

        public UserOnProjectService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        private async Task<UserOnProject> GetNotMappedByIdAsync(int projectId, int userId)
        {
            var model = await _context.UsersOnProjects
                .SingleOrDefaultAsync(uop => uop.ProjectId == projectId && uop.UserId == userId);
            if (model is null)
                throw new InvalidOperationException("Model with such key fields was not found");
            return model;
        }

        public async Task AddAsync(UserOnProjectModel model)
        {
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            model.AdditionDate = DateTimeOffset.UtcNow;
            await _context.UsersOnProjects.AddAsync(_mapper.Map<UserOnProject>(model));
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(int projectId, int userId)
        {
            var model = await GetNotMappedByIdAsync(projectId, userId);
            if (model.Role == UserOnProjectRoles.Owner &&
                !_context.UsersOnProjects.Any(uop => uop.ProjectId == projectId 
                && uop.Role == UserOnProjectRoles.Owner
                && uop.UserId != userId))
                throw new InvalidOperationException("Add a new co-owner first or delete the whole project");
            _context.UsersOnProjects.Remove(model);
            await _context.SaveChangesAsync();
        }

        public IEnumerable<UserOnProjectModelExtended> GetProjectUsers(int projectId)
        {
            var projectTasks = _context.Tasks
                .Include(t => t.Executors)
                .Where(t => t.ProjectId == projectId).AsEnumerable();

            var teamMembers = _context.UsersOnProjects
                .Include(uop => uop.User)
                .AsEnumerable()
                .Where(uop => uop.ProjectId == projectId)
                .Select(uop => new UserOnProjectModelExtended()
                {
                    UserId = uop.UserId,
                    Role = uop.Role,
                    TasksDone = projectTasks
                    .Where(t => t.Executors.Select(ex => ex.UserId).Contains(uop.UserId) && HelperMethods.IsDoneTask(t.Status))
                    .Count(),
                    TasksNotDone = projectTasks
                    .Where(t => t.Executors.Select(ex => ex.UserId).Contains(uop.UserId) && !HelperMethods.IsDoneTask(t.Status))
                    .Count()
                }).ToList();

            return teamMembers.OrderByDescending(t => t.Role).ThenByDescending(t => t.TasksDone).ThenBy(t => t.UserId);
        }

        public async Task<UserOnProjectModel?> GetByIdAsync(int projectId, int userId) 
            => _mapper.Map<UserOnProjectModel?>(await _context.UsersOnProjects
                .SingleOrDefaultAsync(uop => uop.ProjectId == projectId && uop.UserId == userId));

        public bool IsValid(UserOnProjectModel model, out string? firstErrorMessage)
        {
            var result = IModelValidator<UserOnProjectModel>.IsValidByDefault(model, out firstErrorMessage);
            if (!result)
                return false;
            if (!_context.Projects.Any(p => p.Id == model.ProjectId))
            {
                firstErrorMessage = "The project with such an id was not found";
                return false;
            }
            if (!_context.Users.Any(p => p.Id == model.UserId))
            {
                firstErrorMessage = "The user with such an id was not found";
                return false;
            }
            return true;
        }

        public async Task UpdateAsync(UserOnProjectModel model)
        {
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            var existingModel = await GetNotMappedByIdAsync(model.ProjectId, model.UserId);
            if (existingModel.Role == UserOnProjectRoles.Owner &&
                !_context.UsersOnProjects.Any(uop => uop.ProjectId == existingModel.ProjectId
                && uop.Role == UserOnProjectRoles.Owner
                && uop.UserId != existingModel.UserId))
                throw new InvalidOperationException("Add a new co-owner first");
            existingModel = _mapper.Map(model, existingModel);
            _context.UsersOnProjects.Update(existingModel);
            await _context.SaveChangesAsync();
        }

        public async Task<UserOnProjectRoles?> GetRoleOnProjectAsync(int projectId, int userId)
        {
            var uop = await _context.UsersOnProjects
                .SingleOrDefaultAsync(uop => uop.ProjectId == projectId && uop.UserId == userId);
            return uop?.Role;
        }

        public async Task<bool> IsOnProjectAsync(int projectId, int userId)
        {
            return await GetRoleOnProjectAsync(projectId, userId) is not null;
        }
    }
}
