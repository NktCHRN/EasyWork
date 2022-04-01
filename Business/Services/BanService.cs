using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data;
using Data.Entities;
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

        private IEnumerable<Ban> GetNotMappedActiveUserBans(int userId)
        {
            return _context.Bans
                .Where(b => b.UserId == userId && b.DateTo >= DateTime.Now && b.DateFrom <= DateTime.Now);
        }
        private async Task DeleteNotMappedAsync(Ban model)
        {
            _context.Bans.Remove(model);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Adds new ban to DB
        /// </summary>
        /// <param name="model">Model to add</param>
        /// <exception cref="ArgumentException">Thrown if model was not valid</exception>
        public async Task AddAsync(BanModel model)
        {
            model.DateFrom = DateTime.Now;
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            var mapped = _mapper.Map<Ban>(model);
            await _context.Bans.AddAsync(mapped);
            await _context.SaveChangesAsync();
            model.Id = mapped.Id;
        }

        /// <summary>
        /// Deletes ban by id
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="InvalidOperationException">Thrown if model with such an id was not found</exception>
        public async Task DeleteByIdAsync(int id)
        {
            var model = await GetNotMappedByIdAsync(id);
            await DeleteNotMappedAsync(model);
        }

        /// <summary>
        /// Deletes all active bans for user by user id
        /// </summary>
        /// <param name="userId"></param>
        public async Task DeleteActiveUserBansAsync(int userId)
        {
            var bans = GetNotMappedActiveUserBans(userId).ToList();
            for (int i = 0; i < bans.Count; i++)
                await DeleteNotMappedAsync(bans.ElementAt(i));
        }

        /// <summary>
        /// Returns a ban model with given id
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="InvalidOperationException">Thrown if model with such an id was not found</exception>
        /// <returns>A ban model with this id</returns>
        public async Task<BanModel?> GetByIdAsync(int id)
        {
            return _mapper.Map<BanModel?>(await _context.Bans.FindAsync(id));
        }

        /// <summary>
        /// Returns all active bans by user id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>IEnumerable of all active ban models with given user id</returns>
        public IEnumerable<BanModel> GetActiveUserBans(int userId)
        {
            return _mapper.Map<IEnumerable<BanModel>>(GetNotMappedActiveUserBans(userId)).Reverse();
        }

        /// <summary>
        /// Updates ban model
        /// </summary>
        /// <param name="model">Model to update</param>
        /// <exception cref="ArgumentException">Thrown if model was not valid</exception>
        /// <exception cref="InvalidOperationException">Thrown if model with such an id was not found</exception>
        public async Task UpdateAsync(BanModel model)
        {
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            var existingModel = await GetNotMappedByIdAsync(model.Id);
            existingModel = _mapper.Map(model, existingModel);
            _context.Bans.Update(existingModel);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Validates model
        /// </summary>
        /// <param name="model">Model to validate</param>
        /// <param name="firstErrorMessage">String for error message (if model is not valid)</param>
        /// <returns>
        /// False if:
        /// - The expire date is earlier than current date
        /// - Too long hammer
        /// - Not existing user
        /// - Not existing admin
        /// True, otherwise
        /// </returns>
        public bool IsValid(BanModel model, out string? firstErrorMessage)
        {
            var result = IModelValidator<BanModel>.IsValidByDefault(model, out firstErrorMessage);
            if (!result)
                return false;
            if (model.DateTo < model.DateFrom)
            {
                firstErrorMessage = "The expire date of ban cannot be earlier than current date";
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

        /// <summary>
        /// Returns all bans by admin id
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns>IEnumerable of all ban models with given admin id</returns>
        public IEnumerable<BanModel> GetAdminBans(int adminId)
        {
            return _mapper.Map<IEnumerable<BanModel>>(_context.Bans.Where(b => b.AdminId == adminId)).Reverse();
        }

        /// <summary>
        /// Returns all bans by user id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>IEnumerable of all ban models with given user id</returns>
        public IEnumerable<BanModel> GetUserBans(int userId)
        {
            return _mapper.Map<IEnumerable<BanModel>>(_context.Bans.Where(b => b.UserId == userId)).Reverse();
        }

        public bool IsBanned(int userId) => GetNotMappedActiveUserBans(userId).Any();

        public IEnumerable<BanModel> GetLast(int quantity)
        {
            return _mapper.Map<IEnumerable<BanModel>>(_context.Bans.AsEnumerable().TakeLast(quantity)).Reverse();
        }
    }
}
