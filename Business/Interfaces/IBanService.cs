using Business.Models;

namespace Business.Interfaces
{
    public interface IBanService : ICRUD<BanModel>, IModelValidator<BanModel>
    {
        IEnumerable<BanModel> GetActiveUserBans(int userId);

        IEnumerable<BanModel> GetUserBans(int userId);

        IEnumerable<BanModel> GetAdminBans(int adminId);

        IEnumerable<BanModel> GetLast(int quantity);

        bool IsBanned(int userId);

        Task DeleteActiveUserBansAsync(int userId);
    }
}
