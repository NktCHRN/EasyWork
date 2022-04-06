using Business.Models;
using Business.Other;
using Data.Entities;
using Task = System.Threading.Tasks.Task;

namespace Business.Interfaces
{
    public interface IUserOnProjectService : IModelValidator<UserOnProjectModel>
    {
        Task<UserOnProjectModel?> GetByIdAsync(int projectId, int userId);

        Task AddAsync(UserOnProjectModel model);

        Task UpdateAsync(UserOnProjectModel model);

        Task DeleteByIdAsync(int projectId, int userId);

        Task<IEnumerable<UserOnProjectModelExtended>> GetAllProjectUsersAsync(int projectId);  // participants + owner

        Task<UserOnProjectRoles?> GetRoleOnProjectAsync(int projectId, int userId);

        Task<bool> IsOnProjectAsync(int projectId, int userId);
    }
}
