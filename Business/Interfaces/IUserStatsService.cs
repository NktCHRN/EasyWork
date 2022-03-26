using Business.Other;

namespace Business.Interfaces
{
    public interface IUserStatsService
    {
        UserStats GetStatsById(int userId);
    }
}
