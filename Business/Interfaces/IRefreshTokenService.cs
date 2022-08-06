using Business.Models;

namespace Business.Interfaces
{
    public interface IRefreshTokenService : IModelValidator<RefreshTokenModel>
    {
        Task AddAsync(RefreshTokenModel model);

        Task<RefreshTokenModel?> FindAsync(string token, int userId);

        Task DeleteByIdAsync(int id);

        Task DeleteUserTokensAsync(int userId);

        Task UpdateAsync(RefreshTokenModel model);
    }
}
