using Business.Models;

namespace Business.Interfaces
{
    public interface IUserOnProjectService : ICRUD<UserOnProjectModel>
    {
        Task<IAsyncEnumerable<UserOnProjectModel>> GetProjectUsersAsync(int projectId);
    }
}
