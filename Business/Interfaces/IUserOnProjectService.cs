using Business.Models;

namespace Business.Interfaces
{
    public interface IUserOnProjectService : ICRUD<UserOnProjectModel>, IModelValidator<UserOnProjectModel>
    {
        Task<IAsyncEnumerable<UserOnProjectModel>> GetProjectUsersAsync(int projectId);
    }
}
