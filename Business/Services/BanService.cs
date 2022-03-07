using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data;
using Data.Entities;
using System.ComponentModel.DataAnnotations;
using Task = System.Threading.Tasks.Task;

namespace Business.Services
{
    public class BanService : IBanService
    {
        private readonly ApplicationDbContext _context;

        private readonly IMapper _mapper;

        public BanService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private async Task<Ban> GetNotMappedByIdAsync(int id)
        {
            var model = await _context.Bans.FindAsync(id);
            if (model is null)
                throw new InvalidOperationException("Model with such an id was not found");
            return model;
        }

        private IEnumerable<Ban> GetNotMappedUserBans(int userId)
        {
            return _context.Bans.Where(b => b.UserId == userId);
        }
        private async Task DeleteNotMappedAsync(Ban model)
        {
            _context.Bans.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task AddAsync(BanModel model)
        {
            model.DateFrom = DateTime.Now;
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            await _context.Bans.AddAsync(_mapper.Map<Ban>(model));
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(int id)
        {
            var model = await GetNotMappedByIdAsync(id);
            await DeleteNotMappedAsync(model);
        }

        public async Task DeleteUserBansAsync(int userId)
        {
            var bans = GetNotMappedUserBans(userId);
            foreach (var ban in bans)
                await DeleteNotMappedAsync(ban);
        }

        public async Task<BanModel> GetByIdAsync(int id)
        {
            return _mapper.Map<BanModel>(await GetNotMappedByIdAsync(id));
        }

        public IEnumerable<BanModel> GetUserBans(int userId)
        {
            return _mapper.Map<IEnumerable<BanModel>>(GetNotMappedUserBans(userId));
        }

        public async Task UpdateAsync(BanModel model)
        {
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            _context.Bans.Update(_mapper.Map<Ban>(model));
            await _context.SaveChangesAsync();
        }

        public bool IsValid(BanModel model, out string? firstErrorMessage)
        {
            firstErrorMessage = null;
            if (model.DateTo < model.DateFrom)
            {
                firstErrorMessage = "The expire date of ban cannot be earlier than current date";
                return false;
            }
            var validationContext = new ValidationContext(model);
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);
            if (!isValid)
            {
                firstErrorMessage = validationResults.First().ErrorMessage;
                return false;
            }
            var user = _context.Users.Find(model.UserId);
            if (user is null)
            {
                firstErrorMessage = "User was not found";
                return false;
            }
            var admin = _context.Users.Find(model.AdminId);
            if (admin is null)
            {
                firstErrorMessage = "Admin was not found";
                return false;
            }
            return true;
        }

        public IEnumerable<BanModel> GetAdminBans(int adminId)
        {
            return _mapper.Map<IEnumerable<BanModel>>(_context.Bans.Where(b => b.AdminId == adminId));
        }
    }
}
