using Business.Models;

namespace Business.Interfaces
{
    public interface IProjectService : ICRUD<ProjectModel>
    {
        Task<IAsyncEnumerable<ProjectModel>> GetAllAsync();

        Task<IAsyncEnumerable<ProjectModel>> GetUserProjectsAsync(int userId);
    }
}
