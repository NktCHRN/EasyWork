using Business.Models;

namespace Business.Interfaces
{
    public interface ITaskModel : ICRUD<TaskModel>
    {
        Task<IAsyncEnumerable<ReleaseModel>> GetProjectTasksAsync(int projectId);

        Task<IAsyncEnumerable<ReleaseModel>> GetUserTasksAsync(int userId);

        Task<IAsyncEnumerable<ReleaseModel>> GetUserOnProjectTasksAsync(int userOnProjectId);
    }
}
