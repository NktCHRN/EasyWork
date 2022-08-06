using AutoMapper;
using Business.Interfaces;
using Business.Models;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace Business.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly ApplicationDbContext _context;

        private readonly IMapper _mapper;

        public RefreshTokenService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        private async Task<RefreshToken> GetNotMappedByIdAsync(int id)
        {
            var model = await _context.RefreshTokens.FindAsync(id);
            if (model is null)
                throw new InvalidOperationException("Model with such an id was not found");
            return model;
        }

        public async Task AddAsync(RefreshTokenModel model)
        {
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            var mapped = _mapper.Map<RefreshToken>(model);
            await _context.RefreshTokens.AddAsync(mapped);
            await _context.SaveChangesAsync();
            model.Id = mapped.Id;
        }

        public async Task DeleteByIdAsync(int id)
        {
            var model = await GetNotMappedByIdAsync(id);
            _context.RefreshTokens.Remove(model);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserTokensAsync(int userId)
        {
            var tokens = _context.RefreshTokens.Where(t => t.UserId == userId);
            _context.RefreshTokens.RemoveRange(tokens);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshTokenModel?> FindAsync(string token, int userId)
        {
            var found = await _context.RefreshTokens
                .OrderBy(e => e.Id)
                .LastOrDefaultAsync(t => t.Token == token && t.UserId == userId);
            return _mapper.Map<RefreshTokenModel?>(found);
        }

        public bool IsValid(RefreshTokenModel model, out string? firstErrorMessage)
        {
            var result = IModelValidator<RefreshTokenModel>.IsValidByDefault(model, out firstErrorMessage);
            if (!result)
                return false;
            if (!_context.Users.Any(u => u.Id == model.UserId))
            {
                firstErrorMessage = "The user with such an id was not found";
                return false;
            }
            return true;
        }

        public async Task UpdateAsync(RefreshTokenModel model)
        {
            bool isValid = IsValid(model, out string? error);
            if (!isValid)
                throw new ArgumentException(error, nameof(model));
            var existingModel = await GetNotMappedByIdAsync(model.Id);
            if (model.UserId != existingModel.UserId)
                throw new ArgumentException("User id cannot be changed", nameof(model));
            if (model.ExpiryTime != existingModel.ExpiryTime)
                throw new ArgumentException("Expiry time cannot be changed", nameof(model));
            existingModel = _mapper.Map(model, existingModel);
            _context.RefreshTokens.Update(existingModel);
            await _context.SaveChangesAsync();
        }
    }
}
