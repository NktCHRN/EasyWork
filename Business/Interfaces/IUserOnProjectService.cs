using Business.Models;
using Business.Other;

namespace Business.Interfaces
{
    public interface IUserOnProjectService : IModelValidator<UserOnProjectModel>
    {
        Task<UserOnProjectModel> GetByIdAsync(int projectId, int userId);

        Task AddAsync(UserOnProjectModel model);

        Task UpdateAsync(UserOnProjectModel model);

        Task DeleteByIdAsync(int projectId, int userId);

        IEnumerable<UserOnProjectModel> GetProjectUsers(int projectId);          // participants only

        Task<IEnumerable<(int UserId, UserOnProjectRoles Role)>> GetAllProjectUsersAsync(int projectId);  // participants + owner
    }
}
