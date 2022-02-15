using Business.Models;

namespace Business.Interfaces
{
    public interface IBanService : ICRUD<BanModel>
    {
        Task<IAsyncEnumerable<BanModel>> GetUserBansAsync(int userId);

        Task DeleteUserBansAsync(int userId);
    }
}
