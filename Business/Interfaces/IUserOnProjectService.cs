using Business.Models;

namespace Business.Interfaces
{
    public interface IUserOnProjectService : ICRUD<UserOnProjectModel>, IModelValidator<UserOnProjectModel>
    {
        Task<IEnumerable<UserOnProjectModel>> GetProjectUsersAsync(int projectId);
    }
}
