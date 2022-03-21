using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data;
using Data.Entities;
using Microsoft.AspNetCore.Http;
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
            if (!string.IsNullOrEmpty(model.MainPictureFormat))
                throw new ArgumentException("Main picture format should be null or empty at the creation of the project", 
                    nameof(model));
            await _context.Projects.AddAsync(_mapper.Map<Project>(model));
            await _context.SaveChangesAsync();
        }

        public async Task ChangeOwnerByProjectIdAsync(int projectId, int newOwnerId)
        {
            var existingModel = await GetNotMappedByIdAsync(projectId);
            var newOwner = await _context.UsersOnProjects
                .Where(uop => uop.UserId == newOwnerId && uop.ProjectId == existingModel.Id && uop.IsManager)
                .SingleOrDefaultAsync();
            if (newOwner is null)
                throw new InvalidOperationException("User with such an id (new owner) is not a manager on a project right now");

            var oldOwnerId = existingModel.OwnerId;
            // in case if old owner was also registered as user on project
            var oldOwnerOnProject = await _context.UsersOnProjects
                .Where(uop => uop.UserId == existingModel.OwnerId && uop.ProjectId == existingModel.Id)
                .SingleOrDefaultAsync(); 
            if (oldOwnerOnProject is not null)
            {
                _context.UsersOnProjects.Remove(oldOwnerOnProject);
                await _context.SaveChangesAsync();
            }
            //

            existingModel.OwnerId = newOwnerId;
            _context.Projects.Update(existingModel);
            await _context.SaveChangesAsync();

            _context.UsersOnProjects.Remove(newOwner);
            await _context.SaveChangesAsync();

            var oldOwnerAsManager = new UserOnProject()
            {
                ProjectId = existingModel.Id,
                UserId = oldOwnerId,
                IsManager = true
            };
            await _context.UsersOnProjects.AddAsync(oldOwnerAsManager);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(int id)
        {
            var model = await GetNotMappedByIdAsync(id);
            if (!string.IsNullOrEmpty(model.MainPictureFormat))
                await DeleteMainPictureByProjectIdAsync(id);
            _context.Projects.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMainPictureByProjectIdAsync(int projectId)
        {
            var model = await GetNotMappedByIdAsync(projectId);
            if (string.IsNullOrEmpty(model.MainPictureFormat))
                throw new InvalidOperationException("The project does not have any main picture right now");
            try
            {
                _manager.DeleteFile(model.Id.ToString() + '.' + model.MainPictureFormat, Enums.EasyWorkFileTypes.ProjectMainPicture);
                model.MainPictureFormat = null;
                _context.Projects.Update(model);
                await _context.SaveChangesAsync();
            }
            catch (Exception) { }
        }

        public IEnumerable<ProjectModel> GetAll() => _mapper.Map<IEnumerable<ProjectModel>>(_context.Projects);

        public async Task<ProjectModel> GetByIdAsync(int id)
        {
            var model = await _context.Projects
                .Include(m => m.TeamMembers)
                .Include(m => m.Tasks)
                .Include(m => m.Tags)
                .Include(m => m.Releases)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (model is null)
                throw new InvalidOperationException("Model with such an id was not found");
            return _mapper.Map<ProjectModel>(model);
        }

        public IEnumerable<ProjectModel> GetUserProjects(int userId)
        {
            var projectsAsParticipantIds = _context.UsersOnProjects
                .Where(uop => uop.UserId == userId)
                .Select(uop => uop.ProjectId);
            var projectsAsParticipant = _context.Projects
                .Where(p => projectsAsParticipantIds
                    .Contains(p.Id));

            var projectsAsOwner = _context.Projects
                .Where(p => p.OwnerId == userId);

            var united = projectsAsParticipant
                .Union(projectsAsOwner)
                .OrderBy(p => p.Id);
            return _mapper.Map<IEnumerable<ProjectModel>>(united);
        }

        public bool IsValid(ProjectModel model, out string? firstErrorMessage)
        {
            var result = IModelValidator<ProjectModel>.IsValidByDefault(model, out firstErrorMessage);
            if (!result)
                return false;
            if (!_context.Users.Any(u => u.Id == model.OwnerId))
            {
                firstErrorMessage = "The user with such an id was not found";
                return false;
            }
            return true;
        }

        public async Task UpdateAsync(ProjectModel model)
        {
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            var existingModel = await GetNotMappedByIdAsync(model.Id);
            if (model.OwnerId != existingModel.OwnerId)
                throw new ArgumentException("Please, use dedicated method to change the owner of the project", nameof(model));
            if (model.MainPictureFormat != existingModel.MainPictureFormat)
                throw new ArgumentException("Please, use dedicated method to change the main picture", nameof(model));
            if (model.StartDate != existingModel.StartDate)
                throw new ArgumentException("Project's start date cannot be changed", nameof(model));
            existingModel = _mapper.Map(model, existingModel);
            _context.Projects.Update(existingModel);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMainPictureByProjectIdAsync(int projectId, IFormFile image)
        {
            var model = await GetNotMappedByIdAsync(projectId);
            string? oldFileName = null;
            if (!string.IsNullOrEmpty(model.MainPictureFormat))
                oldFileName = model.Id + "." + model.MainPictureFormat;
            var extension = Path.GetExtension(image.FileName);
            if (!_manager.IsValidImageType(extension))
                throw new ArgumentException("Not appropriate file type", image.FileName);
            var newFileName = projectId + extension;
            try
            {
                await _manager.AddFileAsync(image, newFileName, Enums.EasyWorkFileTypes.ProjectMainPicture);
                if (oldFileName != newFileName)
                {
                    model.MainPictureFormat = extension[1..];
                    _context.Projects.Update(model);
                    await _context.SaveChangesAsync();
                    if (!string.IsNullOrEmpty(oldFileName))
                        _manager.DeleteFile(oldFileName, Enums.EasyWorkFileTypes.ProjectMainPicture);
                }
            }
            catch (Exception) { }
        }
    }
}
