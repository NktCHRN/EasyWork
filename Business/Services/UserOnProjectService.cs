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
            await _context.UsersOnProjects.AddAsync(_mapper.Map<UserOnProject>(model));
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(int projectId, int userId)
        {
            var model = await GetNotMappedByIdAsync(projectId, userId);
            _context.UsersOnProjects.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<(int UserId, UserOnProjectRoles Role)>> GetAllProjectUsersAsync(int projectId)
        {
            ICollection<(int UserId, UserOnProjectRoles Role)> teamMembers = await _context.UsersOnProjects
                .Where(uop => uop.ProjectId == projectId)
                .Select(uop => new ValueTuple<int, UserOnProjectRoles>(uop.UserId, 
                    (UserOnProjectRoles)Convert.ToUInt16(uop.IsManager))).ToListAsync();

            var project = await _context.Projects.FindAsync(projectId);
            if (project is not null)
                teamMembers.Add((project.OwnerId, UserOnProjectRoles.Owner));

            return teamMembers.OrderByDescending(t => t.Role).ThenBy(t => t.UserId);
        }

        public async Task<UserOnProjectModel> GetByIdAsync(int projectId, int userId) 
            => _mapper.Map<UserOnProjectModel>(await GetNotMappedByIdAsync(projectId, userId));

        public IEnumerable<UserOnProjectModel> GetProjectUsers(int projectId)
        {
            return _mapper.Map<IEnumerable<UserOnProjectModel>>(_context.UsersOnProjects
                .Where(uop => uop.ProjectId == projectId)
                .OrderByDescending(uop => uop.IsManager)
                .ThenBy(uop => uop.UserId));
        }

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
            existingModel = _mapper.Map(model, existingModel);
            _context.UsersOnProjects.Update(existingModel);
            await _context.SaveChangesAsync();
        }
    }
}
