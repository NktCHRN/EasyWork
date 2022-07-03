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

        private readonly IFileManager _manager;

        public ProjectService(ApplicationDbContext context, IMapper mapper, IFileManager manager)
        {
            _context = context;
            _mapper = mapper;
            _manager = manager;
        }

        private async Task<Project> GetNotMappedByIdAsync(int id)
        {
            var model = await _context.Projects
                .Include(p => p.Tasks)
                .SingleOrDefaultAsync(p => p.Id == id);
            if (model is null)
                throw new InvalidOperationException("Model with such an id was not found");
            return model;
        }

        public async Task AddAsync(ProjectModel model)
        {
            model.StartDate = DateTimeOffset.UtcNow;
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            var mapped = _mapper.Map<Project>(model);
            await _context.Projects.AddAsync(mapped);
            await _context.SaveChangesAsync();
            model.Id = mapped.Id;
        }

        public async Task DeleteByIdAsync(int id)
        {
            var mdl = await GetNotMappedByIdAsync(id);
            var tasks = _context.Tasks.Include(t => t.Files).Where(t => t.ProjectId == id);
            var files = new List<Data.Entities.File>();
            foreach (var task in tasks)
                files.AddRange(task.Files);
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
            _context.Projects.Remove(mdl);
            await _context.SaveChangesAsync();
        }

        public int GetCount() => _context.Projects.Count();

        public async Task<ProjectModel?> GetByIdAsync(int id)
        {
            var model = await _context.Projects
                .Include(m => m.TeamMembers)
                .Include(m => m.Tasks)
                .Include(m => m.Releases)
                .SingleOrDefaultAsync(m => m.Id == id);
            return _mapper.Map<ProjectModel?>(model);
        }

        public IEnumerable<ProjectModel> GetUserProjects(int userId)
        {
            var projectsIds =  _context.UsersOnProjects.AsEnumerable().Reverse()
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

        public async Task<ProjectModel?> GetProjectByActiveInviteCodeAsync(Guid inviteCode)
        {
            return _mapper.Map<ProjectModel?>(await _context.Projects
                .FirstOrDefaultAsync(p => p.InviteCode == inviteCode && p.IsInviteCodeActive));
        }

        public async Task<ProjectModel?> GetProjectByActiveInviteCodeAsync(string? inviteCode)
        {
            var parsed = Guid.TryParse(inviteCode, out Guid result);
            if (!parsed)
                return null;
            return await GetProjectByActiveInviteCodeAsync(result);
        }
    }
}
