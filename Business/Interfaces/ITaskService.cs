using Business.Models;

namespace Business.Interfaces
{
    public interface ITaskService : ICRUD<TaskModel>, IModelValidator<TaskModel>
    {
        Task<IEnumerable<TaskModel>> GetProjectTasksAsync(int projectId);

        Task<IEnumerable<TaskModel>> GetUserTasksAsync(int userId);

        Task<IEnumerable<TaskModel>> GetProjectUserTasksAsync(int projectId, int userId);

        Task AddTagToTaskAsync(int taskId, int tagId);

        Task DeleteTagFromTaskAsync(int taskId, int tagId);
    }
}
