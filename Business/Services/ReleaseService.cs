using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data;
using Data.Entities;
using Task = System.Threading.Tasks.Task;

namespace Business.Services
{
    public class ReleaseService : IReleaseService
    {
        private readonly ApplicationDbContext _context;

        private readonly IMapper _mapper;

        public ReleaseService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private async Task<Release> GetNotMappedByIdAsync(int id)
        {
            var model = await _context.Releases.FindAsync(id);
            if (model is null)
                throw new InvalidOperationException("Model with such an id was not found");
            return model;
        }

        public async Task AddAsync(ReleaseModel model)
        {
            model.Date = DateTime.UtcNow;
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            var mapped = _mapper.Map<Release>(model);
            await _context.Releases.AddAsync(mapped);
            await _context.SaveChangesAsync();
            model.Id = mapped.Id;
        }

        public async Task DeleteByIdAsync(int id)
        {
            var model = await GetNotMappedByIdAsync(id);
            _context.Releases.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task<ReleaseModel?> GetByIdAsync(int id) => _mapper.Map<ReleaseModel?>(await _context.Releases.FindAsync(id));

        public IEnumerable<ReleaseModel> GetProjectReleases(int projectId)
        {
            return _mapper.Map<IEnumerable<ReleaseModel>>(_context.Releases.Where(r => r.ProjectId == projectId))
                .Reverse();
        }

        public bool IsValid(ReleaseModel model, out string? firstErrorMessage)
        {
            var result = IModelValidator<ReleaseModel>.IsValidByDefault(model, out firstErrorMessage);
            if (!result)
                return false;
            if (!_context.Projects.Any(p => p.Id == model.ProjectId))
            {
                firstErrorMessage = "The project with such an id was not found";
                return false;
            }
            return true;
        }

        public async Task UpdateAsync(ReleaseModel model)
        {
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            var existingModel = await GetNotMappedByIdAsync(model.Id);
            if (model.Date != existingModel.Date)
                throw new ArgumentException("Date cannot be changed", nameof(model));
            if (model.ProjectId != existingModel.ProjectId)
                throw new ArgumentException("Project id cannot be changed", nameof(model));
            existingModel = _mapper.Map(model, existingModel);
            _context.Releases.Update(existingModel);
            await _context.SaveChangesAsync();
        }
    }
}
