using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Business.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;

        private readonly IMapper _mapper;

        public ProjectService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private async Task<Project> GetNotMappedByIdAsync(int id)
        {
            var model = await _context.Projects.FindAsync(id);
            if (model is null)
                throw new InvalidOperationException("Model with such an id was not found");
            return model;
        }

        public async Task AddAsync(ProjectModel model)
        {
            model.StartDate = DateTime.Now;
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            await _context.Projects.AddAsync(_mapper.Map<Project>(model));
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(int id)
        {
            var model = await GetNotMappedByIdAsync(id);
            _context.Projects.Remove(model);
            await _context.SaveChangesAsync();
        }

        public int GetCount() => _context.Projects.Count();

        public async Task<ProjectModel> GetByIdAsync(int id)
        {
            var model = await _context.Projects
                .Include(m => m.TeamMembers)
                .Include(m => m.Tasks)
                .Include(m => m.Releases)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (model is null)
                throw new InvalidOperationException("Model with such an id was not found");
            return _mapper.Map<ProjectModel>(model);
        }

        public IEnumerable<ProjectModel> GetUserProjects(int userId)
        {
            var projectsIds = _context.UsersOnProjects.Reverse()
                .Where(uop => uop.UserId == userId)
                .Select(uop => uop.ProjectId);
            var projects = projectsIds
                .Select(p => _context.Projects.
                SingleOrDefault(proj => proj.Id == p));
            return _mapper.Map<IEnumerable<ProjectModel>>(projects);
        }

        public bool IsValid(ProjectModel model, out string? firstErrorMessage)
        {
            var result = IModelValidator<ProjectModel>.IsValidByDefault(model, out firstErrorMessage);
            return result;
        }

        public async Task UpdateAsync(ProjectModel model)
        {
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            var existingModel = await GetNotMappedByIdAsync(model.Id);
            if (model.StartDate != existingModel.StartDate)
                throw new ArgumentException("Project's start date cannot be changed", nameof(model));
            existingModel = _mapper.Map(model, existingModel);
            _context.Projects.Update(existingModel);
            await _context.SaveChangesAsync();
        }
    }
}
