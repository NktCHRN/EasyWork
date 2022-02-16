using Business.Models;

namespace Business.Interfaces
{
    public interface ITaskModel : ICRUD<TaskModel>
    {
        Task<IAsyncEnumerable<TaskModel>> GetProjectTasksAsync(int projectId);

        Task<IAsyncEnumerable<TaskModel>> GetUserTasksAsync(int userId);

        Task<IAsyncEnumerable<TaskModel>> GetUserOnProjectTasksAsync(int userOnProjectId);

        Task AddTagToTaskAsync(int taskId, TagModel tag);

        Task DeleteTagFromTaskAsync(int taskId, int tagId);
    }
}
